using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class ExamReviewViewModel
    {
        public ExamAttempt Attempt { get; set; }
        public List<Question> Questions { get; set; }
        public Dictionary<int, string> StudentAnswers { get; set; }
        public string StudentName { get; set; } 
    }
}
