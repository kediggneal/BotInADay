using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace BotInADay.Lab4_AdaptiveCards.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private string _initialUserText;
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(PromptName);

            return Task.CompletedTask;
        }

        private async Task PromptName(IDialogContext context, IAwaitable<object> result)
        {
            // store what the user said to prompt RootDialog and we'll forward it on after we get their name
            _initialUserText = ((IMessageActivity)await result).Value.ToString();
            PromptDialog.Text(context, AfterName, "What is your name?", "Sorry, I didn't get that.", 3);
        }

        private async Task AfterName(IDialogContext context, IAwaitable<string> result)
        {
            string name = await result;
            // store name in bot session data so we can access it from other dialogs, we will use ths in TriviaRichCardDialog      
            IMessageActivity activity = context.MakeMessage();
            StateClient stateClient = activity.GetStateClient();
            // get the current user data
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
            // upate/set a property called 'name'
            userData.SetProperty<string>("Name", name);
            // send the updated userdata back to the bot server
            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            // Say Hi
            await context.PostAsync($"Hi {name}!");

            // forward the initial users text to QnAMaker
            activity.Text = _initialUserText;
            await context.Forward(new QnADialog(40), AfterQnA, activity, CancellationToken.None);
        }

        /// <summary>
        ///     This is the default handler for any message recieved from the user
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            try
            {
                // try QnAMaker first with a tolerance of 40% match to try to catch a lot of different phrasings
                // the higher the tolerance the more closely the users text must match the questions in QnAMaker
                await context.Forward(new QnADialog(40), AfterQnA, activity, CancellationToken.None);
            }
            catch (Exception e)
            {
                // if an error occured with QnAMaker post it out to the user
                await context.PostAsync(e.Message);
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        ///     This will get called after returning from the QnA
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task AfterQnA(IDialogContext context, IAwaitable<object> result)
        {
            IMessageActivity message = null;
            try
            {
                // our QnADialog returns an IMessageActivity 
                // if the result was something other than an IMessageActivity then some error must have happened
                message = (IMessageActivity)await result;                
            }
            catch (Exception e)
            {
                await context.PostAsync($"QnAMaker: {e.Message}");
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
            // if that messages summary = NO_FOUND then it's time to echo
            if (message.Summary == QnADialog.NOT_FOUND)
            {
                // if they mentioned trivia start a game!
                if (message.Text.ToLowerInvariant().Contains("trivia"))
                {
                    // since we're not needing to pass any messag to start trivia we can use call instead of forward
                    await context.Forward(new TriviaRichCardDialog(), AfterTrivia, message);
                }
                else
                {
                    // echo back to the user, then take over the world!
                    await context.PostAsync($"You said: \"{message.Text}\"");
                    // wait for the next message
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                // display the answer from QnAMaker
                await context.PostAsync(message.Text);
                // wait for the next message
                context.Wait(MessageReceivedAsync);
            }
        }

        /// <summary>
        ///     This will get called after returning from the TriviaRichCardDialog
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="result">The IAwaitable result</param>
        /// <returns></returns>
        private async Task AfterTrivia(IDialogContext context, IAwaitable<object> result)
        {
            // wait for the next message
            context.Wait(MessageReceivedAsync);
        }
    }
}