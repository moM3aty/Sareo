using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class GradeSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentName { get; set; }
        public string SubmissionFilePath { get; set; }

        [Required(ErrorMessage = "الرجاء إدخال درجة.")]
        [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين 0 و 100.")]
        [Display(Name = "الدرجة (من 100)")]
        public int? Grade { get; set; }

        [Display(Name = "ملاحظات للمعلم")]
        public string Feedback { get; set; }
    }
}
