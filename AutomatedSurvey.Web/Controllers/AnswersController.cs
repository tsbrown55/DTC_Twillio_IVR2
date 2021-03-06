using System;
using System.Web.Mvc;
using AutomatedSurvey.Web.Domain;
using AutomatedSurvey.Web.Models;
using AutomatedSurvey.Web.Models.Repository;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace AutomatedSurvey.Web.Controllers
{
    public class AnswersController : TwilioController
    {
        private readonly IRepository<Question> _questionsRepository;
        private readonly IRepository<Answer> _answersRepository;

        public AnswersController()
            : this(
                new QuestionsRepository(),
                new AnswersRepository()) { }

        public AnswersController(
            IRepository<Question> questionsRepository,
            IRepository<Answer> answersRepository
            )
        {
            _questionsRepository = questionsRepository;
            _answersRepository = answersRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(Answer answer)
        {
            _answersRepository.Create(answer);

            var nextQuestion = new QuestionFinder(_questionsRepository).FindNext(answer.QuestionId);
            var response = (nextQuestion != null ? new Response(nextQuestion).Build() : ExitResponse);

            var nextId = answer.QuestionId + 1;
            var url = Url.Action("find", "questions", new { id = nextId });
            var uri = new Uri (url, UriKind.Relative);

            response.Redirect(uri);

            return TwiML(response);
        }

        private static VoiceResponse ExitResponse
        {
            get
            {
                var response = new VoiceResponse();
                response.Say("Thanks for your time. Good bye");
                response.Hangup();
                return response;
            }
        }
    }
}