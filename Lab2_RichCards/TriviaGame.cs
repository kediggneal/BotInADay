using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BotInADay.Lab2_RichCards
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
                answer = 2,
                Question = "What is the worlds largest continent?",
                Choices = new string[] { "Africa", "North America", "Asia", "South America", "Europe", "Australia", "Antartica"}
            },
            new TriviaQuestion()
            {
                index = 1,
                answer = 4,
                Question = "How many rings does Saturn have?",
                Choices = new string[] {"5", "7", "2", "0", "9", "11"}
            },
            new TriviaQuestion()
            {
                index = 2,
                answer = 3,
                Question = "Which is my favorite color?",
                Choices = new string[] {"Red","Green","Yellow","Blue"}
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