using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Check
{
    public class CheckProductFlowerRequest
    {
        public int? ProductQuantity { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? DetailedAddress { get; set; }
    }
}
