using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class UserSubscription
    {
        public int UserSubscriptionId { get; set; }
        public int UserId { get; set; }
        public int SubscriptionTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
