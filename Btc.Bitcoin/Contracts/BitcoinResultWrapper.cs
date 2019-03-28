namespace Btc.Bitcoin.Contracts
{
    public class BitcoinResultWrapper<T>
    {
        public T Result { get; set; }

        public BitcoinApiError Error { get; set; }
    }
}