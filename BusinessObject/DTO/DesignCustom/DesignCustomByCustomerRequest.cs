using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
namespace BusinessObject.DTO.DesignCustom
{
    public class DesignCustomByCustomerRequest
    {
        [FromForm]
        public IFormFile? RequestImage { get; set; }
        public string? RequestDescription { get; set; }
        public string? RequestPrice { get; set; }
        public string? Phone { get; set; }

        public string? RequestOccasion { get; set; }
        public string? RequestMainColor { get; set; }
        public string? RequestFlowerType { get; set; }
        public string? RequestCard { get; set; }

        public string? Note { get; set; }

        public Guid? StoreId { get; set; }

        public string? RecipientName { get; set; }

    }
}
