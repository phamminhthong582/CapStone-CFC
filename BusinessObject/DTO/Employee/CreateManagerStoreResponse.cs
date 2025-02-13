using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.DTO.Employee
{
    public class CreateManagerStoreRequest
    {
        public string? Password { get; set; }

        public string? FullName { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public bool? Gender { get; set; }

        public DateTime? Birthday { get; set; }
        public Guid? RoleId { get; set; }
        public bool? Status { get; set; }
        [FromForm]
        public IFormFile? Avatar { get; set; }

    }
}
