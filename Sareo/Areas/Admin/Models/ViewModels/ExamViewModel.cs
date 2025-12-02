using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class ExamViewModel
    {
        public Exam Exam { get; set; }
        public string CourseTitle { get; set; }
        public ICollection<Question> Questions { get; set; }
    }

}
