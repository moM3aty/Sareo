using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class QuestionFormViewModel
    {
        public Question Question { get; set; }
        public int ExamId { get; set; }
        public string CourseTitle { get; set; }

        [Display(Name = "محتوى السؤال (نص أو صورة ملصقة)")]
        public string QuestionContent { get; set; }


        public List<SelectListItem> CorrectAnswerOptions { get; set; }

        public void GenerateCorrectAnswerOptions()
        {
            var options = new List<SelectListItem>
            {
                new SelectListItem { Value = "A", Text = "الخيار أ" },
                new SelectListItem { Value = "B", Text = "الخيار ب" }
            };

            if (Question.NumberOfOptions >= 3)
            {
                options.Add(new SelectListItem { Value = "C", Text = "الخيار ج" });
            }
            if (Question.NumberOfOptions >= 4)
            {
                options.Add(new SelectListItem { Value = "D", Text = "الخيار د" });
            }
            CorrectAnswerOptions = options;
        }
    }
}
