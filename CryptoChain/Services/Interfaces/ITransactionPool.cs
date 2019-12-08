using CryptoChain.Models;
using System.Collections.ObjectModel;

namespace CryptoChain.Services.Interfaces
{
    public interface ITransactionPool
    {
        void SetTransaction(Transaction transaction);
        Transaction ExistingTransaction(string inputAddress);
        ReadOnlyDictionary<string, Transaction> TransactionMap { get; }
    }
}