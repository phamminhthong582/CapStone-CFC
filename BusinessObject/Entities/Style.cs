using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Style
    {
        public Guid? StyleId {  get; set; }
        public string? Name { get; set; }    
        public string? Note {  get; set; }
        public string? Description { get; set; } 
        public string? Image {  get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool? Status { get; set; }
        public virtual ICollection<ProductCustom> ProductCustoms { get; set; } = new List<ProductCustom>();


    }
}
