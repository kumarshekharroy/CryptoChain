using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
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

        public Transaction CreateTransaction(string recipient, long amount)
        {
            if (amount <= 0) throw new InvalidOperationException("Invalid transaction amount.");

            if (this.Balance < amount)  throw new InvalidOperationException("Transaction amount exceeds balance.");
          

            return new Transaction(senderWallet: this, recipient, amount);

        }

    }
}
