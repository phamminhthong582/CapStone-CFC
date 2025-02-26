using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Style
{
    public  class StyleResponse
    {
        public Guid? StyleId { get; set; }
        public string? Name { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
        public string? CategoryName {  get; set; }
        public string? Image { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? Status { get; set; }
        public bool? Feature { get; set; }

    }
}
