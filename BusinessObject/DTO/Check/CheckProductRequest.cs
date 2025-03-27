using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Check
{
    public class CheckProductRequest
    {
        public List<CheckQuantityAndWeightProduct>? CheckQuantityAndWeightProducts { get; set; }
        public string? City { get; set; }
        public string? District {  get; set; }
        public string? DetailedAddress {  get; set; }
        
    }
}
