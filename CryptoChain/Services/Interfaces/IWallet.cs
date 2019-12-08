using CryptoChain.Models;

namespace CryptoChain.Services.Interfaces
{
    public interface IWallet
    {
        long Balance { get; }
        string PublicKey { get; }

        Transaction CreateTransaction(string recipient, long amount);
        string Sign(string data);
    }
}