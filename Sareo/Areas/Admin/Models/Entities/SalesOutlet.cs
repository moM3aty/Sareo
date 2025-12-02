using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class SalesOutlet
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المحافظة مطلوب")]
        [Display(Name = "المحافظة")]
        public string Governorate { get; set; }

        [Required(ErrorMessage = "اسم المكتبة مطلوب")]
        [Display(Name = "اسم المكتبة")]
        public string BookstoreName { get; set; }

        public virtual ICollection<EducationalMaterial> AvailableMaterials { get; set; }
    }
}
