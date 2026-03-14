using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallProERP.Models.DTOs.CustomerDtos
{
    public class CustomerStatisticsDto
    {
        public int TotalCustomers { get; set; }
        public int NewLeadCount { get; set; }
        public int InterestedCount { get; set; }
        public int OpportunityCount { get; set; }
        public int WonCount { get; set; }
        public int LostCount { get; set; }
    }
}
