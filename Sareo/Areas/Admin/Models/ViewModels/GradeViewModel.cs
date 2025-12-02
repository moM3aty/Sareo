using Microsoft.AspNetCore.Mvc.Rendering;
using Sareoo.Areas.Admin.Models.Entities;
using System.Collections.Generic;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    // ViewModel for listing grades
    public class GradeIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StageName { get; set; }
    }

    // ViewModel for the Create/Edit form
    public class GradeFormViewModel
    {
        public Grade Grade { get; set; }
        public IEnumerable<SelectListItem> Stages { get; set; }
    }

    // ViewModel for the new grouped Index page
    public class GradeGroupedViewModel
    {
        public string StageName { get; set; }
        public List<Grade> Grades { get; set; }
    }
}
