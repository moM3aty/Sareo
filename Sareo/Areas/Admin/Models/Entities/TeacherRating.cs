using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class TeacherRating
    {
        public int Id { get; set; }

        [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين ١ و ٥ نجوم.")]
        [Display(Name = "التقييم")]
        public int Rating { get; set; }

        [StringLength(500)]
        [Display(Name = "التعليق")]
        public string Comment { get; set; }

        [Display(Name = "تاريخ التقييم")]
        public DateTime RatingDate { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
}
