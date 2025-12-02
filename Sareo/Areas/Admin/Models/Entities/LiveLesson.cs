using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public enum LiveLessonStatus { Upcoming, Live, Recorded }
    public enum LivePlatform { Zoom, GoogleMeet, MicrosoftTeams, Other }

    public class LiveLesson
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الدرس مطلوب")]
        [Display(Name = "عنوان الدرس")]
        public string Title { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Display(Name = "صورة الغلاف")]
        public string CoverImageUrl { get; set; }

        [Display(Name = "موعد البدء")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }

        [Display(Name = "المدة (بالدقائق)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "أقصى عدد للطلاب")]
        public int MaxStudents { get; set; }

        [Display(Name = "المنصة")]
        public LivePlatform Platform { get; set; }

        [Display(Name = "رابط الاجتماع")]
        [Url(ErrorMessage = "الرجاء إدخال رابط صحيح")]
        public string MeetingUrl { get; set; }

        public LiveLessonStatus Status { get; set; }
        public int RemindersCount { get; set; }

        [Required(ErrorMessage = "يجب اختيار المعلم")]
        [Display(Name = "المعلم")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "يجب اختيار المادة")]
        [Display(Name = "المادة")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "يجب اختيار الصف الدراسي")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }

        public virtual ICollection<LiveLessonReminder> LiveLessonReminders { get; set; }
    }
}
