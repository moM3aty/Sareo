using System;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المقال مطلوب")]
        [Display(Name = "عنوان المقال")]
        public string Title { get; set; }

        [Display(Name = "محتوى المقال")]
        public string Content { get; set; }

        [Display(Name = "رابط صورة المقال")]
        public string ImageUrl { get; set; }

        [Display(Name = "تاريخ النشر")]
        public DateTime PublishDate { get; set; }

        [Display(Name = "الكلمات المفتاحية")]
        public string Keywords { get; set; }
    }
}
