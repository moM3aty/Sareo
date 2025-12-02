using Sareoo.Areas.Admin.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sareoo.Areas.Admin.Models.ViewModels
{
    public class EducationalMaterialFormViewModel
    {
        public EducationalMaterial EducationalMaterial { get; set; }

        [Display(Name = "صورة الغلاف")]
        public IFormFile CoverImage { get; set; }

        public SelectList Grades { get; set; }
        public SelectList Subjects { get; set; }

        public List<AssignedSalesOutletViewModel> SalesOutlets { get; set; }
    }

    public class AssignedSalesOutletViewModel
    {
        public int OutletId { get; set; }
        public string Name { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class EducationalMaterialIndexViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CoverImageUrl { get; set; }
        public string GradeName { get; set; }
        public string SubjectName { get; set; }
    }
}
