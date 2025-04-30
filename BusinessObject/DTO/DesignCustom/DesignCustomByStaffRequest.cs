using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.DesignCustom
{
    public class DesignCustomByStaffRequest
    {
        public double? ResponsePrice { get; set; }
        [FromForm]
        public IFormFile? ResponseImage { get; set; }
        public string? ResponseDescription { get; set; }
    }
}
