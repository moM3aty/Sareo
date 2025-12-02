using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Stage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المرحلة مطلوب")]
        [StringLength(100)]
        [Display(Name = "اسم المرحلة")]
        public string Name { get; set; }

        public virtual ICollection<Grade> Grades { get; set; }
    }
}
