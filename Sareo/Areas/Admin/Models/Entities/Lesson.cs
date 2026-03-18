using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        [StringLength(200)]
        [Display(Name = "عنوان الدرس")]
        public string Title { get; set; }

        [Display(Name = "رابط فيديو اليوتيوب (اختياري)")]
        [Url(ErrorMessage = "الرجاء إدخال رابط صحيح")]
        public string? VideoUrl { get; set; }

        [Display(Name = "Bunny Library ID (رقم المكتبة)")]
        public string? BunnyLibraryId { get; set; }

        [Display(Name = "Bunny Video ID (رقم الفيديو)")]
        public string? BunnyVideoId { get; set; }

        [Display(Name = "معاينة مجانية للطلاب؟")]
        public bool IsFreePreview { get; set; } = false;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        public virtual ICollection<LessonAttachment> Attachments { get; set; }
        public virtual LessonQuiz LessonQuiz { get; set; }
    }
}