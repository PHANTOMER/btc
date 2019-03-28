using Btc.Contracts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Btc.DataAccess.Context
{
    public class BtcContext : DbContext
    {
        public BtcContext(DbContextOptions<BtcContext> options) : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Wallet> Wallets { get; set; }
    }
}
