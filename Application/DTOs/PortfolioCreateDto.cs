using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PortfolioCreateDto
    {
        [Required(ErrorMessage = "Portfolio name is required.")]
        [MinLength(3, ErrorMessage = "Portfolio name must be at least 3 characters long.")]
        public string Name { get; set; } = string.Empty;
    }
}
