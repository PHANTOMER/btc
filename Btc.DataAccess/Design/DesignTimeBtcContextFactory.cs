using Btc.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Btc.DataAccess.Design
{
    public class DesignTimeAuthContextFactory : IDesignTimeDbContextFactory<BtcContext>
    {
        public BtcContext CreateDbContext(string[] args)
        {
            return new BtcContext(new DbContextOptions<BtcContext>(), "Server=.;Database=btc;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=SSPI");
        }
    }
}
