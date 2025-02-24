using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Employee
{
    public class CourierResponse
    {
        public Guid EmployeeId { get; set; }

        public string? FullName { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public bool? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        public string? IdentificationNumber { get; set; }

        public string? IdentificationFontOfPhoto { get; set; }

        public string? IdentificationBackOfPhoto { get; set; }


        public string RoleName { get; set; }

        public Guid? StoreId { get; set; }

        public bool? Status { get; set; }
        public string? Avatar { get; set; }
        public string? NumberMoto { get; set; }

        public string? ColorMoto { get; set; }

        public string? MotoType { get; set; }
    }
}
