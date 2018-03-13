using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using QnAMakerDialog;
using QnAMakerDialog.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotInADay.Lab4_AdaptiveCards.Dialogs
{

    /// <summary>
    /// Make sure you go to "Manage Nuget Packages" and add QnAMakerDialog by Garry Pretty for this code to work
    /// </summary>
    [Serializable]
    // this is required even though we are reading from the web.config in th QnADialog initilizer below
    // if you forget this tag you will get a 400 bad request error from QnAMaker
    [QnAMakerService("", "")] 
    public class QnADialog : QnAMakerDialog<object>
    {
        public const string NOT_FOUND = "NOT_FOUND"; // when no match is found in QnA maker we'll return a message with this in the summary
        private float _tolerance = 0;

        public QnADialog(float tolerance) : base()
        {
            // initialize the tolerance passed in on instantiation
            _tolerance = tolerance;
            // setup the KnowledgeBaseId and SubscriptionKey from the web.config
            base.KnowledgeBaseId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];
            base.SubscriptionKey = ConfigurationManager.AppSettings["QnASubscriptionKey"];
        }

        /// <summary>
        ///     The DefaultMatchHandler is called whenver any match is found in QnAMaker, no matter how high the score
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="originalQueryText">The text the user sent to the bot</param>
        /// <param name="result">The result returned from the QnAMaker service</param>
        /// <returns></returns>
        public override Task DefaultMatchHandler(IDialogContext context, string originalQueryText, QnAMakerResult result)
        {
            var message = context.MakeMessage(); // create a new message to retun
            message.Summary = NOT_FOUND; // init as NOT_FOUND
            message.Text = originalQueryText; // keep the original user's text in the text in case the calling dialog wants to use it
            float bestMatch = result.Answers.Max(a => a.Score); // find the best score of the matches
            
            if (bestMatch >= _tolerance) // if the best matching score is greater than our tolerance, use it
            {
                message.Summary = "";
                // send back the answer from QnA as the messages text
                message.Text = result.Answers.Where(a => a.Score == bestMatch).FirstOrDefault().Answer;
            }
            // finish the dialog and return the message to the calling dialog
            context.Done(message);
            return Task.CompletedTask;
        }

        /// <summary>
        ///     When no match at all is found in QnA NoMatchHandler is called
        /// </summary>
        /// <param name="context">The current chat context</param>
        /// <param name="originalQueryText">The text the user sent to the bot</param>
        /// <returns></returns>
        public override Task NoMatchHandler(IDialogContext context, string originalQueryText)
        {
            var message = context.MakeMessage(); // create a new message to return
            message.Summary = NOT_FOUND; // mark it as NOT_FOUND
            message.Text = originalQueryText; // keep original text in case the calling dialog needs it
            context.Done(message); // finish the dialog and return the message to the calling dialog
            return Task.CompletedTask;
        }
    }
}
