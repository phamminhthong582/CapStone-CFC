using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class User
    {
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public DateTime? CreateAt { get; set; }
        public Guid? CustomerId { get; set; }

        public DateTime? UpdateAt { get; set; }

        public bool? Status { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Customer? Customer { get; set; }

        public virtual Role? Role { get; set; }
      

    }
}
