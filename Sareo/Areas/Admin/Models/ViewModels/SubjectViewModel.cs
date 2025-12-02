using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sareoo.Areas.Admin.Models.Entities;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for the index page
    public class SubjectIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> GradeNames { get; set; }
    }

    // ViewModel for the Create/Edit form
    public class SubjectFormViewModel
    {
        public Subject Subject { get; set; }
        public List<AssignedGradeViewModel> Grades { get; set; }
    }

    // Helper ViewModel for checkboxes
    public class AssignedGradeViewModel
    {
        public int GradeId { get; set; }
        public string Name { get; set; }
        public bool IsAssigned { get; set; }
    }
}
