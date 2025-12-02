using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class StudentCourse
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }

        [Display(Name = "نسبة التقدم")]
        public int ProgressPercentage { get; set; }

        [Display(Name = "آخر وصول")]
        public DateTime? LastAccessDate { get; set; }

        public virtual Student Student { get; set; }
        public virtual Course Course { get; set; }
    }
}
