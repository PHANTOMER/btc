using System;
using Btc.Contracts.Enums;

namespace Btc.Contracts.Entities
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public string TxId { get; set; }

        public string Address { get; set; }

        public DateTime TxDate { get; set; }

        public decimal Amount { get; set; }

        public int Confirmation { get; set; }

        public Guid WalletId { get; set; }

        public TransactionType TransactionType { get; set; }

        public Wallet Wallet { get; set; }
    }
}