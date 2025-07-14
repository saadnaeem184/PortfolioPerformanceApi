using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class TransactionCreateDto
    {
        [Required(ErrorMessage = "Transaction date is required.")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Transaction quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }
        [Required(ErrorMessage = "Transaction price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Transaction type is required.")]
        public TransactionType Type { get; set; }
    }
}
