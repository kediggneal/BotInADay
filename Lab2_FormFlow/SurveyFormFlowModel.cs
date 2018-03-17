using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BotInADay.Lab_2_1
{
    // form flow will automatically add spaces to camel cased enums when displaying to the user
    // the first value in the enum should always be the "unkown" value
    public enum DifficultyRating {NoAnswer,CornFlakes, Pancakes, GreenEggsAndHam, SpinachFetaFrittata }
    public enum FunRating {NoAnswer,CleaningTheBathroom, AMorningJog, FridayAtSix, Saturday, MyBirthday}
    public enum HowMuch {NoAnswer, NothingAtAll, SomeSpareChange, TradeInMycar, AllOfTheMoney}
    [Serializable]
    public class SurveyFormFlowModel
    {
        public string ageRange;
        public bool playAgain;
        public string yourLocation;
        public DifficultyRating difficultyRating;
        public FunRating funRating;
        // use the optional tag to mark questions as optional, we will conditionally ask this question baesd on the age range selection
        [Optional]
        public HowMuch howMuchWouldYouPay;

        /// <summary>
        /// Call to load the list of age range options dynamically, you can pull values from a database, API, anywhere!
        /// </summary>
        /// <returns></returns>
        private static List<string> GetAgeRangeChoices()
        {
            return new List<string>
            {
                "0 - 15",
                "16 - 32",
                "33 - 55",
                "56 - 79",
                "80+"
            };
        }


        /// <summary>
        ///  This function will be used with FormDialog.FromForm to create a dialog from this form.
        ///  
        ///     The age range uses the FieldReflector class to dynamically load the choice values.
        ///     
        ///     The HowMuch field uses the .Field syntax to define the message as well as a conditional value
        ///     to only ask this question if the person chose an age range that is not 0 - 15 and they said they
        ///     would play again. Pattern language syntax is used to add a custom message and tell how to display choices for how much:
        ///     https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-formflow-pattern-language
        ///     
        ///     AddRemainingFields automatically finds public fields and creates dialog for them
        /// </summary>
        /// <returns></returns>
        public static IForm<SurveyFormFlowModel> BuildForm()
        {
            return new FormBuilder<SurveyFormFlowModel>()
                    .Message("What is your age range?")
                    .Field(new FieldReflector<SurveyFormFlowModel>(nameof(ageRange))
                        .SetType(null)
                        .SetDefine((state, field) =>
                        {
                            foreach (string ageRangeChoice in GetAgeRangeChoices())
                            {
                                field
                                    .AddDescription(ageRangeChoice, ageRangeChoice)
                                    .AddTerms(ageRangeChoice, ageRangeChoice);
                            }
                            return Task.FromResult(true);
                        })
                    )
                    .Field(nameof(playAgain), "Would you ever play this game again?")
                    .Field(nameof(howMuchWouldYouPay),
                            "What would you pay for this game?{||}",
                            (thisForm) => (thisForm.ageRange != "0 - 15" && thisForm.playAgain == true)
                            )
                    .AddRemainingFields()
                    .Message("Thank you! Your choices: {*}")
                    .Build();
        }
    }
}