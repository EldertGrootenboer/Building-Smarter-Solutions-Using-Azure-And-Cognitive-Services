// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples.Models;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class VisitorPictureDialog : CancelAndHelpDialog
    {
        private const string PictureStepMessageText = "Please upload a picture of the visitor. This is used for providing access to the terminal.";
        private const string PictureOptionalStepMessageText = "Would you like to upload a picture? This is used for enhanced access to the terminal.";

        public VisitorPictureDialog()
            : base(nameof(VisitorPictureDialog))
        {
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ConfirmationStepAsync,
                PictureStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            if (!visitDetails.Visitor.IsNew)
            {
                var promptMessage = MessageFactory.Text(PictureOptionalStepMessageText, PictureOptionalStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(true, cancellationToken);
        }

        private async Task<DialogTurnResult> PictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            var doUpload = (bool)stepContext.Result;

            if (visitDetails.Visitor.Picture == null && doUpload)
            {
                var promptMessage = MessageFactory.Text(PictureStepMessageText, PictureStepMessageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(AttachmentPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            return await stepContext.NextAsync(new List<Attachment> {}, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var visitDetails = (VisitDetails)stepContext.Options;

            var attachments = (List<Attachment>)stepContext.Result;

            foreach(var attachment in attachments)
            {
                visitDetails.Visitor.Picture = new VisitorPicture { Uri = attachment.ContentUrl, ContentType = attachment.ContentType };
            }

            return await stepContext.EndDialogAsync(visitDetails, cancellationToken);
        }
    }
}
