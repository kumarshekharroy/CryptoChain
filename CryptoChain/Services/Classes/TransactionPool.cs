using CryptoChain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoChain.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Net;
using Newtonsoft.Json;

namespace CryptoChain.Services.Classes
{
    public class TransactionPool : ITransactionPool
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Dictionary<string, Transaction> _TransactionMap { get; set; }

        public TransactionPool()
        {
            this._TransactionMap = new Dictionary<string, Transaction>();
            SyncTransactionPool();
        }

        private void SyncTransactionPool()
        {
            using var webClient = new WebClient();
            try
            {
                Logger.Info($"Getting latest Transaction Pool from peer node : {Constants.ROOT_NODE_URL}/api/transaction-pool.");
                var response = webClient.DownloadString($"{Constants.ROOT_NODE_URL}/api/transaction-pool");
                var newTransactionMap = JsonConvert.DeserializeObject<Dictionary<string, Transaction>>(response);
                this.SetTransactionMap(newTransactionMap);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error while getting latest Transaction Pool from peer node : {Constants.ROOT_NODE_URL}/api/transaction-pool.");
            }

        }

        public ReadOnlyDictionary<string, Transaction> TransactionMap { get { return new ReadOnlyDictionary<string, Transaction>(this._TransactionMap); } }

        public void SetTransaction(Transaction transaction)
        {
            this._TransactionMap[transaction.ID] = transaction;
        }
        private void SetTransactionMap(Dictionary<string, Transaction> transactionMap)
        {
            this._TransactionMap = transactionMap;
        }

        public Transaction ExistingTransaction(string inputAddress)
        {
            return this._TransactionMap.Values.Where(x => x.Input.Address == inputAddress).FirstOrDefault();
        }

    }
}
