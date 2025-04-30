using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class DesignCustom
    {
        public Guid DesignCustomId { get; set; }
        public string? RequestImage {  get; set; }
        public string? RequestDescription { get; set; }
        public string? RequestOccasion { get; set; }
        public string? RequestMainColor { get; set; }
        public string? RequestFlowerType { get; set; }
        public string? RequestCard { get; set; }
        public string? RequestPrice { get; set; }
        public double? ResponsePrice { get; set; }
        public string? ResponseImage { get; set; }
        public string? ResponseDescription { get; set; }
        public string? Phone {  get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string? Status { get; set; }
        public Guid? OrderId { get; set; }
        public virtual Order? Order { get; set; }
    }
}
