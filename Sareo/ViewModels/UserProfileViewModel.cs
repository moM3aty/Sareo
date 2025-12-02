using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.ViewModels
{
 
    public class UserProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string GradeName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime RegistrationDate { get; set; }

        public int CompletedCoursesCount { get; set; }
        public int InProgressCoursesCount { get; set; }
        public int CertificatesCount { get; set; }
        public double TotalLearningHours { get; set; }
        public double AverageGrade { get; set; }
        public int OverallProgressPercentage { get; set; }

        public List<UserCourseViewModel> InProgressCourses { get; set; }
        public List<UserCourseViewModel> CompletedCourses { get; set; }
        public List<UserCertificateViewModel> Certificates { get; set; }
        public List<AvailableCourseViewModel> AvailableCourses { get; set; }
        public List<UserLiveLessonViewModel> LiveNowLessons { get; set; }
        public List<UserLiveLessonViewModel> UpcomingLessons { get; set; }
        public List<Book> AvailableBooks { get; set; }
        public EditProfileViewModel EditProfileViewModel { get; set; }

    }

    public class UserCourseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TeacherName { get; set; }
        public string CoverImageUrl { get; set; }
        public int ProgressPercentage { get; set; }
        public string Grade { get; set; } 
    }


    public class AvailableCourseViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TeacherName { get; set; }
        public string CoverImageUrl { get; set; }
        public int LessonsCount { get; set; }
        public double TotalHours { get; set; }
        public bool IsSubscribed { get; set; }
    }

 
    public class UserCertificateViewModel
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; }
        public string TeacherName { get; set; }
        public int GradePercentage { get; set; }
        public DateTime IssueDate { get; set; }
        public string CertificateGuid { get; set; }
    }


    public class UserLiveLessonViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string GradeName { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxStudents { get; set; }
        public string CoverImageUrl { get; set; }
        public string MeetingUrl { get; set; }
        public bool IsReminderSet { get; set; }
        public int RemindersCount { get; set; }
        public LiveLessonStatus Status { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "المدينة")]
        public string City { get; set; }

        [Display(Name = "الدولة")]
        public string Country { get; set; }

        [Required(ErrorMessage = "الصف الدراسي مطلوب")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        public IEnumerable<SelectListItem> Grades { get; set; }
    }
}
