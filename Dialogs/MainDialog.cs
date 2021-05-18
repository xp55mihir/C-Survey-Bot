// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.12.2
// Activity reference: https://www.youtube.com/watch?v=TFpjKVYYpbE&list=PLstJWFP49NolWjXHuvOklw5K_MhzRurUy&index=1 

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Providers.Entities;
using System.Xml;
using ToDoBot.Dialogs.Operations;



namespace ToDoBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly ToDoLUISRecognizer _luisRecognizer;
        protected readonly ILogger Logger;

        List<string> questions = new List<string>();
        List<string> choices = new List<string>();
        public static List<List<string>> ListOfChocies = new List<List<string>>();
        public static List<string> responses = new List<string>();
        int XMLrunner = 0;
        int XCR = 0;
        int intro = 0;
        public static int i = 0;
        public static string response = "xxx";
        

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ToDoLUISRecognizer luisRecognizer, ILogger<MainDialog> logger)
            : base(nameof(MainDialog))
        {
            _luisRecognizer = luisRecognizer;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new OtherResponseDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ExtractDataStepAsync,
                IntroStepAsync,
                AskStepAsync,
                ActStepAsync,
                CheckQuestionNumberStepAsync,
                FinalStepAsync,
            })); ;

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private Task<DialogTurnResult> ExtractDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (XMLrunner == 0)
            {
                XmlTextReader xtr = new XmlTextReader(@"C:\Users\Mihir\Desktop\Robonomics Internship\Robonomics AI Project\Project-related Proof-Of-Concept\P.O.C.-5 [Working version]\SurveyBotTrial-5\Questions.xml");
                while (xtr.Read())
                {
                    if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "Question")
                    {
                        string s1 = xtr.ReadElementString();
                        questions.Add(s1);
                    }
                }
                XMLrunner++;
            }

            if (XCR == 0)
            {
                XmlTextReader xtr = new XmlTextReader(@"C:\Users\Mihir\Desktop\Robonomics Internship\Robonomics AI Project\Project-related Proof-Of-Concept\P.O.C.-5 [Working version]\SurveyBotTrial-5\AnswerChoices.xml");
                int a = 0;
                while (xtr.Read())
                {
                    if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "AC")
                    {
                        int b = a;
                        int c = 0;
                        string s1 = xtr.ReadElementString();
                        choices.Add(s1);
                        a++;
                        c++;
                        
                        int k = 0;
                        while (xtr.Read() && k == 0)
                        {
                            if (xtr.NodeType == XmlNodeType.Element && xtr.Name == "AC")
                            {
                                string s2 = xtr.ReadElementString();
                                choices.Add(s2);
                                a++;
                                c++;
                            }
                            else
                            {
                                k = 1;
                            }
                        }

                        ListOfChocies.Add(choices.GetRange(b, c));
                    }
                }
                XCR++;
            }


            return stepContext.NextAsync(null, cancellationToken);               
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (intro == 0)
            {
                Thread.Sleep(5000);
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Hello. I will be gathering information about your organization to assess your " +
                "organization's readiness for a Digital Transformation Project."), cancellationToken);
                Thread.Sleep(4000);
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(@"For each question in this survey, you will be presented with multiple options" +
                " from which you will select one which responds most accurately to the question. If you select the" + 
                " last option which reads 'Other ...', type and send the information you feel will most accurately" +
                " respond to the question."), cancellationToken);
                Thread.Sleep(10000);
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("I hope you are ready to take the survey. Let's begin."), cancellationToken);
                Thread.Sleep(3000);
                intro++;
            }

            return await stepContext.NextAsync(null, cancellationToken);

        }

        private async Task<DialogTurnResult> AskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(questions[i]), cancellationToken);

            List<string> optionsList = ListOfChocies[i];
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = optionsList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(optionsList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userDetails = (User)stepContext.Options;
            stepContext.Values["Response"] = ((FoundChoice)stepContext.Result).Value;
            response = (string)stepContext.Values["Response"];

            if (response == ListOfChocies[i][ListOfChocies[i].Count() - 1])
            {
                return await stepContext.BeginDialogAsync(nameof(OtherResponseDialog), new User(), cancellationToken);
            }
            else
            {
                responses.Add(response);
                i++;
                return await stepContext.NextAsync(null, cancellationToken);
            }


            
        }

        private async Task<DialogTurnResult> CheckQuestionNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userDetails = (User)stepContext.Options;
            if (i == questions.Count())
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, userDetails, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
            MessageFactory.Text("You have reached the end of this survey. Thank you for your time."), cancellationToken);

            XmlDocument Xdoc = new XmlDocument();
            Xdoc.Load(@"C:\Users\Mihir\Desktop\Robonomics Internship\Robonomics AI Project\Project-related Proof-Of-Concept\P.O.C.-5 [Working version]\SurveyBotTrial-5\QuestionsAnswers.xml");

            for (int m = 0; m < questions.Count(); m++)
            {
                int n = m + 1;
                XmlNode tgtNode = Xdoc.SelectSingleNode("Questionnaire/QA[@id=" + n.ToString() + "]/Response");
                tgtNode.InnerText = responses[m];
            }

            Xdoc.Save(@"C:\Users\Mihir\Desktop\Robonomics Internship\Robonomics AI Project\Project-related Proof-Of-Concept\P.O.C.-5 [Working version]\SurveyBotTrial-5\QuestionsAnswers.xml");

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken); 
        }
    }
}
