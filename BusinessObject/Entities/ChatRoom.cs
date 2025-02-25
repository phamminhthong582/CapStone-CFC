using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class ChatRoom
    {
        public Guid? ChatRoomId {  get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Status { get; set; } 
        public DateTime? CreateAt {  get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();  
        public virtual Employee? Employee { get; set; }
        public virtual Customer? Customer { get; set; }



    }
}
