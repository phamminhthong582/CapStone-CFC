using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Flower
{
    public class FlowerResponse
    {
        public Guid FlowerId { get; set; }
        
        public string? FlowerName { get; set; }

        public double? Price { get; set; }

        public Guid? StoreId { get; set; }

        public string? Image { get; set; }

        public int? Quantity { get; set; }

        public Guid? CategoryId { get; set; }

        public string? Description { get; set; }

        public int? Sold { get; set; }
    }
}
