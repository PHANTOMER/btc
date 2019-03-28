using Btc.Common.Extensions;
using Btc.Contracts.Entities;
using Microsoft.EntityFrameworkCore;

namespace Btc.DataAccess.Context
{
    public class BtcContext : DbContext
    {
        private readonly string _connectionString;

        public BtcContext(DbContextOptions<BtcContext> options) : base(options)
        {
        }

        public BtcContext(DbContextOptions<BtcContext> options, string connectionString) : base(options)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!_connectionString.IsNullOrEmpty())
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Wallet> Wallets { get; set; }
    }
}
