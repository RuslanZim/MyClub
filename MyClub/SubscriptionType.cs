using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class SubscriptionType
    {
        public int SubscriptionTypeId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public int DurationDays  { get; set; }  
        public decimal Price { get; set; }  
        public DateTime CreatedAt { get; set; }
    }
}
