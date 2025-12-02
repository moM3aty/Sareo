using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class ExamAttemptAnswer
    {
        public int Id { get; set; }
        public int ExamAttemptId { get; set; }
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }

        [ForeignKey("ExamAttemptId")]
        public virtual ExamAttempt ExamAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}