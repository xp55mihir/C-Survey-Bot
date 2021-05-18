using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



namespace ToDoBot.Dialogs.Operations
{
    public class OtherResponseDialog : ComponentDialog
    {
        
        

        public OtherResponseDialog() : base(nameof(OtherResponseDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                PromptUserResponseStepAsync,
                StoreUserResponseStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            
            InitialDialogId = nameof(WaterfallDialog);
        }

       

        private async Task<DialogTurnResult> PromptUserResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please type and send " + MainDialog.ListOfChocies[MainDialog.i][MainDialog.ListOfChocies[MainDialog.i].Count() - 1])
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> StoreUserResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userDetails = (User)stepContext.Options;
            stepContext.Values["Response"] = (string)stepContext.Result;
            MainDialog.response = (string)stepContext.Values["Response"];
            MainDialog.responses.Add(MainDialog.response);
            MainDialog.i++;
            
            return await stepContext.EndDialogAsync(userDetails, cancellationToken);
        }

        
    }
}