using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public partial class Employee
    {
        public Guid EmployeeId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? StoreId { get; set; }
        public virtual User? User { get; set; }
        public virtual Store? Store { get; set; }

    }
}
