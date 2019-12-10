using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoChain.Services.Classes
{
    public class Wallet : IWallet
    {

        public long Balance { get; private set; }
        private AsymmetricCipherKeyPair KeyPair { get; set; }
        public string PublicKey { get; private set; }

        public Wallet()
        {
            this.Balance = Constants.STARTING_BALANCE;

            this.KeyPair = EllipticCurve.GenerateKeys();
            this.PublicKey = this.KeyPair.PublicKey();

        }

        public string Sign(string data)
        {
            return this.KeyPair.Sign(data);
        }

        public Transaction CreateTransaction(string recipient, long amount, IReadOnlyCollection<Block> chain)
        {
            if (chain != null)
                this.Balance = Wallet.CalculateBalance(chain, address: this.PublicKey);

            if (amount <= 0) throw new InvalidOperationException("Invalid transaction amount.");

            if (this.Balance < amount) throw new InvalidOperationException("Transaction amount exceeds balance.");


            return new Transaction(senderWallet: this, recipient, amount);

        }
        public static long CalculateBalance(IReadOnlyCollection<Block> chain, string address)
        {
            bool hasConductedAnyTransaction = false;

            long outputsTotal = 0;//total received 
            foreach (var block in chain.Reverse().SkipLast(1)) //As We recalculate balance on each transaction.So instead of traversing whole blockchain we will start from end and traverse till last transacted block.
            {
                foreach (var transaction in block.Data)
                {
                    if (transaction.Input.Address == address)
                        hasConductedAnyTransaction = true;

                    if (transaction.OutputMap.TryGetValue(address, out var outputAmount))
                        outputsTotal += outputAmount;
                }
                if (hasConductedAnyTransaction)
                    break;

            }
            return hasConductedAnyTransaction ? outputsTotal : Constants.STARTING_BALANCE + outputsTotal;
        }
        long IWallet.CalculateBalance(IReadOnlyCollection<Block> chain, string address)
        {
            return Wallet.CalculateBalance(chain, address);
        }
    }
}
