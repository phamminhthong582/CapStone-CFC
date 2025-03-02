using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Notification
    {
        public Guid? NotificationId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? MessageId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public virtual Message Message { get; set; }  

    }
}
