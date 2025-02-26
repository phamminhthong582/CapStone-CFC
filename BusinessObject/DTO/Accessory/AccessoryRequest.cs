using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Accessory
{
    public class AccessoryRequest
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
        public double? Price { get; set; }
        public Guid? CategoryId { get; set; }

        public string? Description { get; set; }
        [FromForm]
        public IFormFile? Image { get; set; }
        public bool? Status { get; set; }
        public bool? Feature { get; set; }

    }
}
