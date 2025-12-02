using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<PublicTeacherViewModel> Teachers { get; set; }
        public IEnumerable<PublicStageViewModel> Stages { get; set; }
        public IEnumerable<PublicEducationalMaterialViewModel> EducationalMaterials { get; set; }

    }

    public class PublicStageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GradeRange { get; set; }
        public List<string> SubjectNames { get; set; }
        public int StudentCount { get; set; }
    }
}
