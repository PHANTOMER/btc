using System;

namespace Btc.Contracts.Dto
{
    public class TransactionDto
    {
        public DateTime Date { get; set; }

        public string Address { get; set; }

        public decimal Amount { get; set; }

        public int Confirmation { get; set;  }
    }
}