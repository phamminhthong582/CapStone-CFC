using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Message
    {
        public Guid? MessageId { get; set; }
        public Guid? ChatRoomId { get; set; }
        public Guid? SenderId {  get; set; }
        public Guid? ReceiveId { get; set; }
        public string? MessageType {  get; set; }
        public string? Content { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual ChatRoom? ChatRoom { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();  // Quan hệ 1-n với Notification

    }
}
