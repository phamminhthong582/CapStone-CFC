using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Style
{
    public  class StyleRequest
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public string? Description { get; set; }
        [FromForm]
        public IFormFile? Image { get; set; }
        public bool? Status { get; set; }

    }
}
