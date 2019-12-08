using CryptoChain.Services.Classes;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Models
{
    public class Transaction
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private Clock clock;

        public string ID { get; set; }
        public Dictionary<string, long> OutputMap { get; set; }
        public Input Input { get; set; }
        public Transaction()
        {

        }
        public Transaction(Wallet senderWallet, string recipient, long amount)
        {
            this.ID = Guid.NewGuid().ToString();
            this.clock = new Clock();
            this.OutputMap = this.CreateOutputMap(senderWallet, recipient, amount);
            this.Input = this.CreateInput(senderWallet, this.OutputMap);
        }

        private Dictionary<string, long> CreateOutputMap(Wallet senderWallet, string recipient, long amount)
        {
            var outputMap = new Dictionary<string, long>();
            outputMap[recipient] = amount;
            outputMap[senderWallet.PublicKey] = senderWallet.Balance - amount;
            return outputMap;
        }
        private Input CreateInput(IWallet senderWallet, Dictionary<string, long> outputMap)
        {
            return new Input
            {
                Timestamp = clock.UtcNow,
                Amount = senderWallet.Balance,
                Address = senderWallet.PublicKey,
                Signature = senderWallet.Sign(outputMap.SerializeObject())
            };
        }

        public static bool Validate(Transaction transaction)
        {
            var outputTotal = transaction.OutputMap.Values.Sum();
            if (transaction.Input.Amount != outputTotal)
            {
                Logger.Info($"Invalid transaction from {transaction.Input.Address}");
                return false;
            }
            if (!EllipticCurve.VerifySignature(publicKey: transaction.Input.Address, data: transaction.OutputMap.SerializeObject(), signature: transaction.Input.Signature))
            {
                Logger.Info($"Invalid signature from {transaction.Input.Address}");
                return false;
            }
            return true;
        }
        public void Update(IWallet senderWallet, string recipient, long amount)
        {
            if(amount<=0) throw new InvalidOperationException("Invalid transaction amount.");

            if (this.OutputMap[senderWallet.PublicKey] < amount) throw new InvalidOperationException("Transaction amount exceeds balance.");

            if (this.OutputMap.TryGetValue(recipient, out var existingAmt))
            {
                this.OutputMap[recipient] = existingAmt + amount;
            }
            else
            {
                this.OutputMap[recipient] = amount; 
            }
             
            this.OutputMap[senderWallet.PublicKey] -= amount;

            this.Input = this.CreateInput(senderWallet: senderWallet, outputMap: this.OutputMap);
        }


    }
}
