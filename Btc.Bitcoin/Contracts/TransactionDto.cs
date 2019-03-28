namespace Btc.Bitcoin.Contracts
{
    public enum TransactionCategory
    {
        Skipped = 0,
        Receive = 1,
        Send = 2
    }

    public class TransactionDto
    {
        public string TxId { get; set; }

        public string Address { get; set; }

        public decimal Amount { get; set; }

        public string Category { get; set; }

        public int Confirmations { get; set; }

        public string TimeReceived { get; set; }
    }
}
