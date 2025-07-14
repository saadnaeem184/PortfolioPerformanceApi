using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Domain.Entities
{
    public class Asset
    {
        public Guid Id { get; set; }
        public Guid PortfolioId { get; set; } // Foreign key to Portfolio
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AssetType Type { get; set; }
        // Navigation property for transactions.
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
