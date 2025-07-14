using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public DateTime Date { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public TransactionType Type { get; set; }
    }

}
