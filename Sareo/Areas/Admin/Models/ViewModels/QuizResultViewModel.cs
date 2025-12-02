using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class QuizResultViewModel
    {
        public LessonQuizAttempt Attempt { get; set; }
        public List<LessonQuizQuestion> Questions { get; set; }
        public Dictionary<int, string> StudentAnswers { get; set; }
        public string StudentName { get; set; }
    }
}
