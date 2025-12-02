using System.ComponentModel.DataAnnotations.Schema;

namespace Sareoo.Areas.Admin.Models.Entities
{
    public class LiveLessonReminder
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
        public int LiveLessonId { get; set; }
        [ForeignKey("LiveLessonId")]
        public virtual LiveLesson LiveLesson { get; set; }
    }
}
