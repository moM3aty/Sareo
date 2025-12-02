using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class Subject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المادة مطلوب")]
        [Display(Name = "اسم المادة")]
        public string Name { get; set; }

        public virtual ICollection<Grade> Grades { get; set; }

        public virtual ICollection<Teacher> Teachers { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<LiveLesson> LiveLessons { get; set; }
        public virtual ICollection<EducationalMaterial> EducationalMaterials { get; set; }

    }
}