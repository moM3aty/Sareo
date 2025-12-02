using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class ExamAttempt
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; } 
        public int Score { get; set; } 
        public DateTime AttemptDate { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }

        public virtual ICollection<ExamAttemptAnswer> Answers { get; set; }
    }
}
