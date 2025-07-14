using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{

    public class AssetUpdateDto
    {
        [Required(ErrorMessage = "Asset symbol is required.")]
        [MinLength(1, ErrorMessage = "Asset symbol must be at least 1 character long.")]
        public string Symbol { get; set; } = string.Empty;
        [Required(ErrorMessage = "Asset name is required.")]
        [MinLength(3, ErrorMessage = "Asset name must be at least 3 characters long.")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Asset type is required.")]
        public AssetType Type { get; set; }
    }
}
