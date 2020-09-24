// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Models;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class RegisterVisitorDialog : CancelAndHelpDialog
    {
        private const string ShipStepMessageText = "Which ship would is going to be visited?";
        private const string NameStepMessageText = "What is the name of the visitor?";
        private const string ReasonStepMessageText = "What is the reason of the visit?";
        private const string EmailStepMessageText = "On which email address can the visitor be reached?";

        public RegisterVisitorDialog()
            : base(nameof(RegisterVisitorDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ShipStepAsync,
                NameStepAsync,
                VisitDateTimeStepAsync,
                ReasonStepAsync,
                EmailStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShipStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            if (visitDetails.Ship == null)
            {
                var promptMessage = MessageFactory.Text(ShipStepMessageText, ShipStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(visitDetails.Ship, cancellationToken);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            visitDetails.Ship = (string)stepContext.Result;

            if (visitDetails.Visitor.Name == null)
            {
                var promptMessage = MessageFactory.Text(NameStepMessageText, NameStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(visitDetails.Visitor.Name, cancellationToken);
        }

        private async Task<DialogTurnResult> VisitDateTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            visitDetails.Visitor.Name = (string)stepContext.Result;

            if (visitDetails.Visit.DateTime == null || IsAmbiguous(visitDetails.Visit.DateTime))
            {
                return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), visitDetails.Visit.DateTime, cancellationToken);
            }

            return await stepContext.NextAsync(visitDetails.Visit.DateTime, cancellationToken);
        }

        private async Task<DialogTurnResult> ReasonStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            visitDetails.Visit.DateTime = (string)stepContext.Result;

            if (visitDetails.Visit.Reason == null)
            {
                var promptMessage = MessageFactory.Text(ReasonStepMessageText, ReasonStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(visitDetails.Visit.Reason, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            visitDetails.Visit.Reason = (string)stepContext.Result;

            if (visitDetails.Visitor.Email == null)
            {
                var promptMessage = MessageFactory.Text(EmailStepMessageText, EmailStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(visitDetails.Visitor.Email, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            visitDetails.Visitor.Email = (string)stepContext.Result;

            var visitDateMessage = new TimexProperty(visitDetails.Visit.DateTime).ToNaturalLanguage(DateTime.Now);
            var messageText = $"Please confirm that {visitDetails.Visitor.Name} will be visiting the ship {visitDetails.Ship} for {visitDetails.Visit.Reason} by {visitDateMessage}. Is this correct?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var visitDetails = (VisitDetails)stepContext.Options;

                return await stepContext.EndDialogAsync(visitDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
