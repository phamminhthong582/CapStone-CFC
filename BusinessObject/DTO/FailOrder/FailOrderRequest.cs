using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.FailOrder
{
    public class FailOrderRequest
    {
        public string? ReasonFail { get; set; }

        [FromForm]
        public IFormFile? ImageFail { get; set; }
        public DateTime? TimeDelay { get; set; }
        //public double? RefundPrice { get; set; }
        public bool? Wallet { get; set; }
    }
}
