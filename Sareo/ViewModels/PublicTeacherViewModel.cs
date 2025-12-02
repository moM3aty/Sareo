using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class PublicTeacherViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string JobTitle { get; set; }
        public string ExperienceSummary { get; set; }
        public int ExperienceYears { get; set; }
        public string SubjectsSummary { get; set; }
        public int TotalStudents { get; set; }
        public double AverageRating { get; set; }
    }
}
