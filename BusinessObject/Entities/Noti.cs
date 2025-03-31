using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Noti
    {
        public Guid NotiId { get; set; }
        public Guid? FromUserId { get; set; }
        public Guid? ToUserId { get; set; }
        public Guid? RelatedId { get; set; }
        public string? Type { get; set; }
        public string? Message { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? Status { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
