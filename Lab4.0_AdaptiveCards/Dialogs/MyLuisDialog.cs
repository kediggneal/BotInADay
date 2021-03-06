﻿using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;
using AdaptiveCards;

namespace BotInADay.Lab4_AdaptiveCards
{
    [Serializable]
    public class MyLuisDialog : LuisDialog<string> // explicitly state we will return a string using context.done(string)
    {

        public MyLuisDialog() : base(SetupLuisService())
        {

        }

        protected override Task DispatchToIntentHandler(IDialogContext context, IAwaitable<IMessageActivity> item, IntentRecommendation bestIntent, LuisResult result)
        {
            return base.DispatchToIntentHandler(context, item, bestIntent, result);
        }

        protected override Task<string> GetLuisQueryTextAsync(IDialogContext context, IMessageActivity message)
        {
            return base.GetLuisQueryTextAsync(context, message);
        }

        protected override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            return base.MessageReceived(context, item);
        }
        /// <summary>
        ///     Setup any values of the LuisModelAttribute that you cannot set in the constructor
        /// </summary>
        /// <returns>The LuisService to use in the base constructor of this LuisDialog</returns>
        public static LuisService SetupLuisService()
        {
            LuisModelAttribute attribute = new LuisModelAttribute(
                ConfigurationManager.AppSettings["LuisAppId"],
                ConfigurationManager.AppSettings["LuisAPIKey"],
                domain: ConfigurationManager.AppSettings["LuisAPIHostName"]);
#if DEBUG
            attribute.Staging = true;
#endif
            return new LuisService(attribute);
        }

        [LuisIntent("Happy")]
        public async Task HappyIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm happy you're happy. I'm happy too. We're both happy.");

            context.Done("");
        }

        // Should be invoked when the user asks for help with no specific information
        // such as "what can you helpl me with?"
        [LuisIntent("Sad")]
        public async Task SadIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Oh man, that's too bad. You should be happy like me, it's much better!");
            context.Done("");
        }

        // Should be invoked when the user asks something generic and includes a recognized application name
        // such as "what can you do in SDMPlus?"
        [LuisIntent("Nuetral")]
        public async Task NuetralIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Yea, yea, it is what it is.");
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            context.Done("none");
        }
    }
}