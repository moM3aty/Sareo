using Sareoo.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المعلم مطلوب")]
        [StringLength(100)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [StringLength(100)]
        [Display(Name = "المسمى الوظيفي")]
        public string JobTitle { get; set; }

        [Display(Name = "ملخص الخبرة")]
        public string ExperienceSummary { get; set; }

        [Display(Name = "سنوات الخبرة")]
        public int ExperienceYears { get; set; }

        [Display(Name = "صورة الملف الشخصي")]
        public string ProfileImageUrl { get; set; }

        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<LiveLesson> LiveLessons { get; set; }
        public virtual ICollection<TeacherRating> Ratings { get; set; }
        public virtual ICollection<Book> Books { get; set; }
    }
}
