using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PortfolioDetailDto : PortfolioDto
    {
        public List<AssetDto> Assets { get; set; } = new List<AssetDto>();
    }
}
