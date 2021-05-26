using System;
using System.Collections.Generic;

#nullable disable

namespace MVCOrder.Models
{
    public partial class Limit
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Quantity { get; set; }
    }
}
