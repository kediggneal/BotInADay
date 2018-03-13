using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BotInADay.Lab4_AdaptiveCards.Dialogs
{
    [Serializable]
    public class TriviaRichCardDialog : IDialog<object>
    {
        private TriviaGame _game = null;

        public async Task StartAsync(IDialogContext context)
        {
            // post a message to the user right away letting them know they have started a game of trivia
            await context.PostAsync("Welcome to Trivia!");

            context.Wait(AfterName);
        }

        /// <summary>
        ///     Here we'll set the players name returned from the prompt and then initialize the game
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterName(IDialogContext context, IAwaitable<object> result)
        {
            // we stored the user's name in the bot session data in RootDialog, now we will retrieve it
            IMessageActivity activity = (IMessageActivity)await result;
            StateClient stateClient = activity.GetStateClient();
            // get the current user data
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            // upate/set a property called 'name'
            string name = userData.GetProperty<string>("Name");

            _game = new TriviaGame(name);

            await context.PostAsync($"Ready or not, {name}, Let's play!");
            // most the question and choices as a hero card
            await context.PostAsync(MakeChoiceCard(context, _game.CurrentQuestion()));
            // wait for the answer
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        ///     Here we'll check the users answer and post the next question until there are no more questions
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = (IMessageActivity)await result;
            int usersAnswer = -1;
            if (int.TryParse(activity.Text, out usersAnswer))
            {
                await context.PostAsync($"You chose: {activity.Text}");
                if (_game.Answer(usersAnswer))
                {
                    await context.PostAsync("Correct!");
                }
                else
                {
                    await context.PostAsync("Sorry, that's wrong :-(");
                }
                await context.PostAsync($"Your score is: {_game.Score()}/{_game._questions.Count}. Next question!");
                TriviaQuestion nextQuestion = _game.MoveToNextQuestion();
                if (nextQuestion != null)
                {
                    await context.PostAsync(MakeChoiceCard(context, nextQuestion));
                    context.Wait(MessageReceivedAsync);
                }
                else
                {
                    await context.PostAsync("That's it! Thanks for playing :-)");
                    // see if the user will take our survey
                    PromptDialog.Confirm(context, AfterAskAboutSurvey,
                            new PromptOptions<string>(
                                "Would you like to take a survey?",
                                "Sorry, I didnt't get that",
                                "Hmm...it seems I am having some difficult, let's forget about that survey.",
                                null, 3));
                }
            }
            else
            {
                try
                {
                    // send to luis to see if we can pick up the users intet
                    await context.Forward(new MyLuisDialog(), AfterLuis, activity, CancellationToken.None);
                }
                catch (Exception e)
                {
                    await context.PostAsync($"Tried to start LUIS but got: {e.Message}");
                }
            }
        }

        public async Task AfterLuis(IDialogContext context, IAwaitable<string> message)
        {
            // we don't have to do any casting becuase we explicitly said we will return a string in MyLuisDialog<string>
            string val = await message; 

            if (val == "none") // only show the not programmed message if LUIS found no intent
            {
                await context.PostAsync("I am only programmed to accept numbers as trivia answers :-(");
            }
            context.Wait(MessageReceivedAsync);
        }

        private async Task AfterAskAboutSurvey(IDialogContext context, IAwaitable<bool> result)
        {
            bool takeSurvey = await result;
            if (takeSurvey)
            {
                var survey = new FormDialog<SurveyFormFlowModel>(new SurveyFormFlowModel(), SurveyFormFlowModel.BuildForm, FormOptions.PromptInStart, null);

                context.Call<SurveyFormFlowModel>(survey, AfterSurvey);
            }
            else
            {
                context.Done("");
            }
        }

        private async Task AfterSurvey(IDialogContext context, IAwaitable<SurveyFormFlowModel> result)
        {
            SurveyFormFlowModel survey = await result;
            context.Done("");
        }

        private IMessageActivity MakeChoiceCard(IDialogContext context, TriviaQuestion question)
        {
            var activity = context.MakeMessage();
            // make sure the attachments have been initialized, we use the attachments to add buttons to the activity message
            if (activity.Attachments == null)
            {
                activity.Attachments = new List<Attachment>();
            }

            var actions = new List<CardAction>();
            int choiceIndex = 0;

            foreach (string item in question.Choices)
            {
                actions.Add(new CardAction
                {
                    Title = $"({choiceIndex}) {item}",
                    Value = $"{choiceIndex}",
                    Type = ActionTypes.PostBack // PostBack means the Value will be sent back to the dialog as if the user typed it but it will be hidden from the chat window
                });
                choiceIndex++;
            }
            // create a hero card to "hold" the buttons and add it to the message activities attachments
            activity.Attachments.Add(
                new HeroCard
                {
                    Title = $"{question.index}. {question.Question}",
                    Buttons = actions
                }.ToAttachment()
            );

            return activity;
        }
    }
}