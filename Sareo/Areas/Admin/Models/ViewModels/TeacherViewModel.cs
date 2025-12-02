using Microsoft.AspNetCore.Http;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for listing teachers
    public class TeacherIndexViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public int ExperienceYears { get; set; }
        public string ProfileImageUrl { get; set; }
    }



        public class TeacherFormViewModel
        {
            public Teacher Teacher { get; set; }

            [Display(Name = "صورة الملف الشخصي")]
            public IFormFile? ProfileImage { get; set; }

            [Display(Name = "المواد التي يدرسها")]
            public List<AssignedSubjectViewModel> Subjects { get; set; }

            [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
            [EmailAddress]
            [Display(Name = "البريد الإلكتروني (للدخول)")]
            public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة (اتركه فارغاً لعدم التغيير)")]
        public string? NewPassword { get; set; }
    }

        public class AssignedSubjectViewModel
        {
            public int SubjectId { get; set; }
            public string Name { get; set; }
            public bool IsAssigned { get; set; }
        }
    
}


