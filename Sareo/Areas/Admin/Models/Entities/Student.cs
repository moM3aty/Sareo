using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sareoo.Models;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Student
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(150)]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "المؤهل الدراسي")]
        public string? Qualification { get; set; }

        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessage = "يجب اختيار الصف الدراسي")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        [Display(Name = "الحساب مفعل")]
        public bool IsActive { get; set; }

        [Display(Name = "المدينة")]
        public string City { get; set; }

        [Display(Name = "الدولة")]
        public string Country { get; set; }

        public DateTime? LastAccessDate { get; set; }
        public string ApplicationUserId { get; set; }

        [ForeignKey("GradeId")]
        public virtual Grade Grade { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }
        public virtual ICollection<StudentCourse> StudentCourses { get; set; }
        public virtual ICollection<TeacherRating> GivenRatings { get; set; }
        public virtual ICollection<LiveLessonReminder> LiveLessonReminders { get; set; }

        public virtual ICollection<LessonQuizAttempt> LessonQuizAttempts { get; set; }
    }
}
