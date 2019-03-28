using System;
using System.Collections.Generic;

namespace Btc.Contracts.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Balance { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
