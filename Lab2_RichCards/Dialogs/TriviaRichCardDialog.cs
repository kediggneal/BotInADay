using BotInADay.Lab2_RichCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lab2_RichCards.Dialogs
{
    [Serializable]
    public class TriviaRichCardDialog : IDialog<object>
    {
        private TriviaGame _game = null;

        public async Task StartAsync(IDialogContext context)
        {
            // post a message to the user right away letting them know they have started a game of trivia
            await context.PostAsync("Welcome to Trivia!");

            context.Wait(PromptForName);
        }

        private async Task PromptForName(IDialogContext context, IAwaitable<object> result)
        {
            // get the players name using prompt and start the game
            PromptDialog.Text(context, AfterName, "What is your name?","I'm sorry, I didn't get that",3);
        }

        /// <summary>
        ///     Here we'll set the players name returned from the prompt and then initialize the game
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterName(IDialogContext context, IAwaitable<string> result)
        {
            string name = await result;
            _game = new TriviaGame(name);

            await context.PostAsync($"Hi {name}, Let's play...");
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
            await context.PostAsync($"You chose: {activity.Text}");
            int usersAnswer = -1;
            if (int.TryParse(activity.Text, out usersAnswer))
            {
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
                    context.Done("");
                }
            }
            else
            {
                await context.PostAsync("I didn't quite get that, I am only programmed to accept numbers :-(");
                context.Wait(MessageReceivedAsync);
            }
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