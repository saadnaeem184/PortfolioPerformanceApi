using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AssetDetailDto : AssetDto
    {
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
    }
}
