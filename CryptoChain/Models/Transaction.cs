using CryptoChain.Services.Classes;
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
        public string ID { get; set; }

        public class InputMap
        {
            public DateTime Timestamp { get; set; }
            public long Amount { get; set; }
            public string Address { get; set; }
            public string Signature { get; set; }
        }
        public Dictionary<string, long> OutputMap { get; set; }
        public InputMap Input { get; set; }
        private Clock clock;
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
        private InputMap CreateInput(Wallet senderWallet, Dictionary<string, long> outputMap)
        {
            //var input = new Dictionary<string, object>();
            //input["timestamp"] = clock.UtcNow.ToString();
            //input["amount"] = senderWallet.Balance;
            //input["address"] = senderWallet.PublicKey;
            //input["signature"] = senderWallet.Sign(outputMap.SerializeObject());
            return new InputMap {
                Timestamp= clock.UtcNow,
                Amount= senderWallet.Balance,
                Address= senderWallet.PublicKey,
                Signature= senderWallet.Sign(outputMap.SerializeObject())
            };
        }

        public static bool ValidateTransaction(Transaction transaction)
        {
            var outputTotal = transaction.OutputMap.Values.Sum();
            if(transaction.Input.Amount!= outputTotal)
            {
                Logger.Info($"Invalid transaction from {transaction.Input.Address}");
                return false;
            }
            if(!EllipticCurve.VerifySignature(publicKey: transaction.Input.Address,data:transaction.OutputMap.SerializeObject(),signature:transaction.Input.Signature))
            {
                Logger.Info($"Invalid signature from {transaction.Input.Address}");
                return false;
            }
            return true;
        }
    }
}
