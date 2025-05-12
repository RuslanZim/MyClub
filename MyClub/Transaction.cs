using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClub
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } // Сумма
        public string OperationType { get; set; } // Доход или Расход
        public string Comment { get; set; } // Комментарий
        public int? UserId { get; set; }

    }
}
