using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lab2_AIS_Server
{
    public class CarAuctionContext:DbContext
    {
        public CarAuctionContext(): base("DbConnection") 
        {
            
        }
        public DbSet<CarAuction> CarAuctions { get; set; }
    }
}
