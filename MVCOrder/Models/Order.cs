using System;
using System.Collections.Generic;

#nullable disable

namespace MVCOrder.Models
{
    public partial class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ServiceId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public int? Quantity { get; set; }
        public int? TotalAmount { get; set; }

        public virtual Service Service { get; set; }
        public virtual AspNetUser User { get; set; }
    }
}
