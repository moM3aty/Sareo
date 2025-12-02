using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class LessonQuizAttemptAnswer
    {
        public int Id { get; set; }
        public int LessonQuizAttemptId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }

        [ForeignKey("LessonQuizAttemptId")]
        public virtual LessonQuizAttempt LessonQuizAttempt { get; set; }
        [ForeignKey("QuestionId")]
        public virtual LessonQuizQuestion Question { get; set; }
    }
}
