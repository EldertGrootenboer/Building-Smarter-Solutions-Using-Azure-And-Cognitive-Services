// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Handlers;
using Microsoft.BotBuilderSamples.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly VisitorRegistrationRecognizer _luisRecognizer;
        private readonly VisitorHandler _visitorHandler;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(VisitorRegistrationRecognizer luisRecognizer, RegisterVisitorDialog registerVisitorDialog, VisitorPictureDialog visitorPictureDialog, VisitorHandler visitorHandler, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            _visitorHandler = visitorHandler;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(registerVisitorDialog);
            AddDialog(visitorPictureDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                ActStepAsync,
                RetrievePictureStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "Welcome to Rotterdam Tank Terminal, how can I help you today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the RegisterVisitorDialog path with an empty instance.
                return await stepContext.BeginDialogAsync(nameof(RegisterVisitorDialog), new RegisterVisitorDialog(), cancellationToken);
            }

            // Call LUIS and gather any potential registration details. (Note the TurnContext has the response to the prompt.)
            var luisResult = await _luisRecognizer.RecognizeAsync<BuildingSmarterSolutionsUsingCognitiveServices>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case BuildingSmarterSolutionsUsingCognitiveServices.Intent.Register_visitor:
                    // Initialize VisitorDetails with any entities we may have found in the response.
                    var visitDetails = new VisitDetails
                    {
                        Ship = luisResult.Ship,
                        Visitor = new Visitor
                        {
                            Name = luisResult.VisitorName,
                            Email = luisResult.Email,
                        },
                        Visit = new Visit
                        {
                            DateTime = luisResult.VisitDateTime,
                            Reason = luisResult.Reason
                        }
                    };

                    // Run the RegisterVisitorDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
                    return await stepContext.BeginDialogAsync(nameof(RegisterVisitorDialog), visitDetails, cancellationToken);

                default:
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent}).";
                    var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> RetrievePictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // If the child dialog ("RegisterVisitorDialog") was cancelled, the user failed to confirm or if the intent wasn't correct the Result here will be null.
            if (stepContext.Result is VisitDetails visitDetails)
            {
                var response = await _visitorHandler.RegisterAsync(visitDetails);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(responseContent))
                {
                    var responseMessage = MessageFactory.Text(responseContent, responseContent, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(responseMessage, cancellationToken);
                }

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                {
                    visitDetails.Visitor.IsNew = response.StatusCode == HttpStatusCode.Accepted;
                    visitDetails.Visit.RegistrationCompleted = response.StatusCode == HttpStatusCode.OK;
                    return await stepContext.BeginDialogAsync(nameof(VisitorPictureDialog), visitDetails, cancellationToken);
                }
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is VisitDetails visitDetails)
            {
                // Upload picture if available
                if (visitDetails.Visitor.Picture != null)
                {
                    await _visitorHandler.UploadPictureAsync(visitDetails);
                }

                // Check if visit registration was completed
                if (visitDetails.Visit.RegistrationCompleted)
                {
                    var visitDateMessage = new TimexProperty(visitDetails.Visit.DateTime).ToNaturalLanguage(DateTime.Now);
                    var messageText = $"Your visit for {visitDateMessage} has been registered, please don't forget to bring PPEs.";
                    var message = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(message, cancellationToken);
                }
            }
            
            // Restart the main dialog with a different message the second time around
            var promptMessage = "What else can I do for you?";
            return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        }
    }
}
