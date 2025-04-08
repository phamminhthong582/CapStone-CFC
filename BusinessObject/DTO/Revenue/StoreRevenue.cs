using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Revenue
{
    public class StoreRevenue
    {
        public Guid StoreId { get; set; }
        public string? StoreName { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Address { get; set; }
        public double? Revenue { get; set; }

    }
}
