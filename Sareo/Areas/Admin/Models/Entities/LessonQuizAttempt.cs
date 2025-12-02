using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class LessonQuizAttempt
    {
        public int Id { get; set; }
        public int LessonQuizId { get; set; }
        public int StudentId { get; set; }
        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public DateTime AttemptDate { get; set; }

        [ForeignKey("LessonQuizId")]
        public virtual LessonQuiz LessonQuiz { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        public virtual ICollection<LessonQuizAttemptAnswer> Answers { get; set; }
    }
}
