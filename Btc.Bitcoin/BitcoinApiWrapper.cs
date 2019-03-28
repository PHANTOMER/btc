using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Btc.Bitcoin.Contracts;
using Btc.Common.Extensions;
using Btc.Contracts;
using Btc.Contracts.Configuration;
using Btc.Contracts.Dto;
using Btc.Contracts.Entities;
using Btc.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Btc.Bitcoin
{
    public class BitcoinApiWrapper : IBitcoinApiWrapper
    {
        // hack: couldn't find any reliable solution to get ALL transactions
        private const int VeryLargeNumberOfTransactions = 99999;

        private const string JsonRpcVersion = "1.0";
        private readonly ILogger<BitcoinApiWrapper> _logger;
        private readonly IOptions<BitcoinApiConfig> _options;

        public BitcoinApiWrapper(ILogger<BitcoinApiWrapper> logger, IOptions<BitcoinApiConfig> options)
        {
            _logger = logger;
            _options = options;
        }

        public async Task<string> TrySendBtcAsync(SendBtcDto btc, string wallet)
        {
            return await this.TryExecuteAsync<string>(BitcoinApiMethods.SendToAddress, wallet, btc.Address, btc.Amount).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Transaction>> TryListTransactionsAsync(string walletName)
        {
            var result = await this.TryExecuteAsync<Contracts.TransactionDto[]>(BitcoinApiMethods.ListTransactions, walletName, "*", VeryLargeNumberOfTransactions, 0).ConfigureAwait(false);
            return result
                .Select(x => new
                {
                    Transaction = x,
                    Category = x.Category.ToEnum<TransactionCategory>()
                })
                .Where(x => x.Category != TransactionCategory.Skipped)
                .Select(x => new Transaction()
                {
                    TransactionType = (TransactionType) x.Category,
                    TxId = x.Transaction.TxId,
                    TxDate = x.Transaction.TimeReceived.ToULong().ToDateTime(),
                    Address = x.Transaction.Address,
                    Confirmation = x.Transaction.Confirmations,
                    Amount = x.Transaction.Amount,
                });
        }

        public async Task<IEnumerable<Wallet>> TryListWalletsWithBalancesAsync()
        {
            var result = await this.TryExecuteAsync<string[]>(BitcoinApiMethods.ListWallets).ConfigureAwait(false);
            var wallets = await Task.WhenAll(result.Select(async walletName =>
            {
                decimal balance = await this.TryExecuteAsync<decimal>(BitcoinApiMethods.GetBalance, walletName).ConfigureAwait(false);
                return new Wallet()
                {
                    Name = walletName,
                    Balance = balance
                };
            })).ConfigureAwait(false);

            return wallets;
        }

        private async Task<TResult> TryExecuteAsync<TResult>(string method, string wallet = "", params object[] parameters)
        {
            using (var client = new WebClient())
            {
                string baseAddress = _options.Value.Address;
                string address = wallet.IsNullOrEmpty() ? baseAddress : $"{baseAddress}/wallet/{wallet}";

                client.Headers.Add(HttpRequestHeader.ContentType, "application/json-rpc");
                client.Credentials = new NetworkCredential(_options.Value.RpcUserName, _options.Value.RpcPassword);

                var payload = new JObject();
                string id = Guid.NewGuid().ToString("N");

                payload.Add(new JProperty("jsonrpc", JsonRpcVersion));
                payload.Add(new JProperty("id", id));
                payload.Add(new JProperty("method", method));

                var paramsObj = new JArray(parameters);

                payload.Add(new JProperty("params", paramsObj));
                string payloadString = payload.ToString();

                try
                {
                    var result = await client.UploadStringTaskAsync(address, payloadString).ConfigureAwait(false);
                    var wrapper = result.FromJson<BitcoinResultWrapper<TResult>>();
                    return wrapper.Result;
                }
                catch (WebException exception)
                {
                    var responseStream = exception.Response?.GetResponseStream();

                    if (responseStream != null)
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var responseText = await reader.ReadToEndAsync().ConfigureAwait(false);
                            var wrapper = responseText.FromJson<BitcoinResultWrapper<TResult>>();

                            bool isGenericError = !wrapper?.Error?.NotEnoughFunds ?? true;

                            // in case Balance < Amount + Fee, that is unknown beforehand
                            throw new Exception(isGenericError ? Errors.GenericNetworkError : Errors.NotEnoughFundsError);
                        }
                    }

                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception occured while calling {address}. Payload: {payloadString}");
                    throw new Exception(Errors.GenericNetworkError);
                }
            }
        }
    }
}
