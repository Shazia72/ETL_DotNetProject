using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETLProject
{
    public class OrderRow
    {
        [Column("Id")]
        public long OrderNumber { get; set; }
        public int CustomerId { get; set; }
        public string Description { get; set; } = null!;
        public string CustomerName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
