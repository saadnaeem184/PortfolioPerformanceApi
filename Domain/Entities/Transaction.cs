using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; } // Foreign key to Asset
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; } // Price per unit at the time of transaction
        public TransactionType Type { get; set; }
    }
}
