using CryptoChain.Models;
using System.Collections.Generic;

namespace CryptoChain.Services.Interfaces
{
    public interface IWallet
    {
        long Balance { get; }
        string PublicKey { get; }

        Transaction CreateTransaction(string recipient, long amount, IReadOnlyCollection<Block> chain);
        string Sign(string data);
        long CalculateBalance(IReadOnlyCollection<Block> chain, string address);
    }
}