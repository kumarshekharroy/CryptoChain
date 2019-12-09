using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Services.Classes
{
    public class TransactionMiner : ITransactionMiner
    {
        //private readonly ILogger<TransactionMiner> _ILogger;
        //private readonly IClock _IClock;
        private readonly IBlockChain _IBlockChain;
        private readonly IRedis _IRedis;
        private readonly IWallet _IWallet;
        private readonly ITransactionPool _ITransactionPool;


        public TransactionMiner(IBlockChain blockChain, IRedis redis, IWallet wallet, ITransactionPool transactionPool)
        {  
            this._IBlockChain = blockChain;
            this._IRedis = redis;
            this._IWallet = wallet;
            this._ITransactionPool = transactionPool;
        }

        public async Task MineTransaction()
        {
            //ToDo
            //get transaction pool's valid transactions
            //generate the miner reward
            //add a block consisting of these transactions to the blockchain 
            //broadcast the updated  blockchain 
            //clear the pool

            var validTransactions = _ITransactionPool.ValidTransactions();
            var minerReward = Transaction.RewardTransaction(this._IWallet);
            validTransactions.Add(minerReward);
            _IBlockChain.AddBlock(validTransactions);
            await _IRedis.BroadcastChain();
            _ITransactionPool.ClearBlockchainTransaction(_IBlockChain.LocalChain);

        }
    }
}
