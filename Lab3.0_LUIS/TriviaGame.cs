using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotInADay.Lab3_LLUIS
{
    [Serializable]
    public class TriviaGame
    {
        private string _playersName;
        private int _currentQuestion = 0;
        private int[] _usersAnswers = new int[] { -1, -1, -1 };

        public List<TriviaQuestion> _questions = new List<TriviaQuestion>
        {
            new TriviaQuestion()
            {
                index = 0,
                answer = 4,
                Question = "How many pieces of contemporary art is in Microsoft's collection?",
                Choices = new string[] { "0", "25", "500", "2000", "5000"}
            },
            new TriviaQuestion()
            {
                index = 1,
                answer = 2,
                Question = "In 2016, Microsoft made a major breakthrough, equaling that of humans, in what?",
                Choices = new string[] {"Writing Song Lyrics", "Derby Car Racing", "Speech Recognition", "Predicting American Idol Winners"}
            },
            new TriviaQuestion()
            {
                index = 2,
                answer = 0,
                Question = "Approximately how much money does Microsoft spend on R&D?",
                Choices = new string[] {"$11 billion","$1 million","$100K","None"}
            }
        };

        public TriviaGame(string playersName)
        {
            _playersName = playersName;
        }

        public TriviaQuestion CurrentQuestion()
        {
            return _questions.Where(q => q.index == _currentQuestion).FirstOrDefault();
        }

        public TriviaQuestion MoveToNextQuestion()
        {
            _currentQuestion++;
            if (_currentQuestion < _questions.Count())
            {
                return CurrentQuestion();
            }
            else
            {
                _currentQuestion--;
                return null;
            }
        }
        public TriviaQuestion MoveToPreviousQuestion()
        {
            _currentQuestion--;
            if (_currentQuestion > 0)
            {
                return CurrentQuestion();
            }
            else
            {
                _currentQuestion = 0;
                return null;
            }
        }
        public TriviaQuestion MoveToFirstQuestion()
        {
            _currentQuestion = 0;
            return CurrentQuestion();
        }
        public bool Answer(int answer)
        {
            _usersAnswers[_currentQuestion] = answer;
            return _usersAnswers[_currentQuestion] == _questions[_currentQuestion].answer;
        }
        public int Score()
        {
            return _questions.Where(q => _usersAnswers[q.index] == q.answer).Count();
        }

        
    }

    [Serializable]
    public class TriviaQuestion
    {
        public int index { get; set; }
        public int answer { get; set; }
        public string Question { get; set; }
        public string[] Choices { get; set; }
    }
}