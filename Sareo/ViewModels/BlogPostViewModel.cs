using System;
using System.Collections.Generic;

namespace Sareoo.ViewModels
{
    public class BlogPostViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string ImageUrl { get; set; }
        public string ShortDescription { get; set; }
        public List<string> Keywords { get; set; }
    }
}