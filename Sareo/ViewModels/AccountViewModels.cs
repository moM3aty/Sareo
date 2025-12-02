using Sareoo.Areas.Admin.Models.Entities;
using Sareoo.Areas.Admin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "صيغة رقم الهاتف غير صحيحة")]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }


        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; }
        public string City { get; set; }        
        public string Country { get; set; }
        [Required(ErrorMessage = "يجب اختيار الصف الدراسي")]
        [Display(Name = "الصف الدراسي")]
        public int GradeId { get; set; }

        public IEnumerable<SelectListItem> Grades { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "تذكرني؟")]
        public bool RememberMe { get; set; }
    }

   
}
