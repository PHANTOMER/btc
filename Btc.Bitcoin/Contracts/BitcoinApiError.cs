namespace Btc.Bitcoin.Contracts
{
    public class BitcoinApiError
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public bool NotEnoughFunds => Code == BitcoinApiErrorCodes.TransactionFeeRequired ||
                                                  Code == BitcoinApiErrorCodes.InsufficientFunds;
    }
}