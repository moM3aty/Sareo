using Microsoft.AspNetCore.Mvc.Rendering;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for listing students
    public class StudentIndexViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string GradeName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
    }

    // ViewModel for the Create/Edit form
    public class StudentFormViewModel
    {
        public Student Student { get; set; }

        [Display(Name = "الصف الدراسي")]
        public IEnumerable<SelectListItem> Grades { get; set; }
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string? NewPassword { get; set; }


    }

    // ViewModel for the Student Profile/Details page
    public class StudentProfileViewModel
    {
        public Student Student { get; set; }
        public List<StudentCourseProgressViewModel> CoursesProgress { get; set; }
        public List<CertificateInfoViewModel> Certificates { get; set; }
        public List<LessonQuizAttemptViewModel> QuizAttempts { get; set; }
        public List<ExamAttemptViewModel> FinalExamAttempts { get; set; }
        public int CompletedCourses { get; set; }
        public int CertificatesCount { get; set; }
        public double TotalLearningHours { get; set; }
        public double AverageGrade { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public DateTime? LastAccessDate { get; set; }
    }
    public class ExamAttemptViewModel
    {
        public int AttemptId { get; set; }
        public string CourseTitle { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }
    public class LessonQuizAttemptViewModel
    {
        public int AttemptId { get; set; }
        public string LessonTitle { get; set; }
        public string CourseTitle { get; set; }
        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public DateTime AttemptDate { get; set; }
    }
    // ViewModel for displaying a single course progress
    public class StudentCourseProgressViewModel
    {
        public string CourseTitle { get; set; }
        public int ProgressPercentage { get; set; }
        public string Status { get; set; }

        public int CourseHours { get; set; }
        public DateTime? LastAccessDate { get; set; }
        public double Grade { get; set; }
    }

    // ViewModel for displaying certificate info in the profile
    public class CertificateInfoViewModel
    {
        public int CertificateId { get; set; }
        public string CourseTitle { get; set; }
        public string CertificateGuid { get; set; }

        public DateTime IssueDate { get; set; }
    }
}
