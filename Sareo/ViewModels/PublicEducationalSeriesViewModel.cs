namespace Sareoo.ViewModels
{
    public class PublicEducationalMaterialViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public string PdfFilePath { get; set; }
        public int Downloads { get; set; }
        public int PageCount { get; set; } 
        public double Rating { get; set; }
        public string GradeName { get; set; }
        public string SubjectName { get; set; }
    }
}
