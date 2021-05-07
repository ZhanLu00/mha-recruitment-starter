// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;


namespace Microsoft.BotBuilderSamples
{
    public class StarterDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public StarterDialog(UserState userState, IBotTelemetryClient telemetryClient)
            : base(nameof(StarterDialog))
        {
            this.TelemetryClient = telemetryClient;
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            // This array defines how your steps will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                StepOneAsync,
                StepTwoAsync,
                StepThreeAsync,
                StepFourAsync,
                StepFiveAsync,
                FinalStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            // AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        // wrap every blocks of bot messages in step functions
        private static async Task<DialogTurnResult> StepOneAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // send message 'Welcome to...' to user synchronously
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Welcome to the MHA recruitment starter bot!")
                , cancellationToken);
            
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"The purpose is to show you some example of what you can do with the bot")
                , cancellationToken);
            
            // end this function by asking user a yes/no question
            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                new PromptOptions { 
                    Prompt = MessageFactory.Text("✍  This is a confirm prompt, you can reply by clicking or typing “yes” or “no”  ✍") 
                    }, 
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> StepTwoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { 
                    Prompt = MessageFactory.Text("✍ This is a text prompt, reply anything to proceed ✍") 
                    }, 
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> StepThreeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // you can the latest user reply by checking stepContext.Result
            string userResponse = (string) stepContext.Result;

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("You just typed: " + userResponse), cancellationToken);
            
            // choice prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions { 
                    Prompt = MessageFactory.Text("✍ This is choice prompt. Reply by click on or type in one of the following options  ✍"), 
                    Choices = ChoiceFactory.ToChoices(new List<string> { "An option", "Another option", "Not an option" })
                    }, 
                    cancellationToken);
        }

        private static async Task<DialogTurnResult> StepFourAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {   
            // flow control here
            FoundChoice userResponse = (FoundChoice) stepContext.Result;
            switch (userResponse.Value){
                case "An option":
                    await stepContext.Context.SendActivityAsync(
                          MessageFactory.Text("Common choice")
                          , cancellationToken);
                    break;
                case "Another option": 
                    await stepContext.Context.SendActivityAsync(
                          MessageFactory.Text("Safe choice")
                          , cancellationToken);
                    break;
                case "Not an option":
                    await stepContext.Context.SendActivityAsync(
                          MessageFactory.Text("Expected")
                          , cancellationToken);
                    break;
                default:
                    await stepContext.Context.SendActivityAsync(
                          MessageFactory.Text("wrong option")
                          , cancellationToken);
                    break;
            }

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Let's pause for 10s before proceding")
                , cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);

        }

        private static async Task<DialogTurnResult> StepFiveAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // wait 10s before proceed with next messages
            Task.Delay(10000).Wait();

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"Adding delays in between bot messages makes the conversation feels more real")
                , cancellationToken);
            
            // return null when no need for user to reply 
            return await stepContext.NextAsync(null, cancellationToken);

        }

    
        private static async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text($"This is END of the starter bot, have fun ;)")
                        , cancellationToken);
            // end the conversation
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}