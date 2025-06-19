using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class InventoryItem
    {
        public int InventoryId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
    }
}
