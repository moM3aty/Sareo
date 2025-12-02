using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class TakeLessonQuizViewModel
    {
        public LessonQuiz Quiz { get; set; }
        public Dictionary<int, string> Answers { get; set; }
    }
}