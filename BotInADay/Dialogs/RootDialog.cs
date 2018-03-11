using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BotInADay.Lab1.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        /// <summary>
        /// This functoin is called when the converstaion starts.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task StartAsync(IDialogContext context)
        {
            // setup a function to handle whenever the next message is recieved
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        ///  This is the default handler for any incoming messages. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            // echo back what the user said
            await context.PostAsync($"You said: \"{activity.Text}\"");
            // wait for the next message
            context.Wait(MessageReceivedAsync);
        }
    }
}