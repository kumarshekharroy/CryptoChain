using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CryptoChain.Services.Classes
{
    public class BlockChain : IBlockChain
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private List<Block> localChain;



        public BlockChain()
        {
            this.localChain = new List<Block> { Block.Genesis() };

            SyncChain();
        }

        private void SyncChain()
        {
            using var webClient = new WebClient();
            try
            {
                Logger.Info($"Getting latest chain from peer node : {Constants.ROOT_NODE_URL}/api/blocks.");
                var response = webClient.DownloadString($"{Constants.ROOT_NODE_URL}/api/blocks");
                var newChain = JsonConvert.DeserializeObject<ReadOnlyCollection<Block>>(response);
                this.ReplaceChain(newChain);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error while getting latest chain from peer node : {Constants.ROOT_NODE_URL}/api/blocks.");
            }

        }



        public ReadOnlyCollection<Block> LocalChain { get { return this.localChain.AsReadOnly(); } }

        public void AddBlock(List<Transaction> data)
        {
            var newBlock = Block.MineBlock(localChain[localChain.Count - 1], data);

            this.localChain.Add(newBlock);


        }

         bool ValidateTransactionData(ReadOnlyCollection<Block> chain)
        {
            bool hasPassedAllChecks = true;

            foreach (var block in chain.Skip(1))
            { 
                var rewardTransactionCount = 0;
                 
                HashSet<Transaction> uniqueTransactions = new HashSet<Transaction>();
                foreach (var transaction in block.Data)
                {
                    if (transaction.Input.Address == Constants.MINING_REWARD_INPUT)
                    {
                        if (++rewardTransactionCount > 1)
                        {
                            hasPassedAllChecks = false;
                            Logger.Info("Multiple Miners Rewards transactions found in the block");
                            break;
                        } 
                        if (transaction.OutputMap.Values.FirstOrDefault() != Constants.MINING_REWARD)
                        {
                            hasPassedAllChecks = false;
                            Logger.Info("Miners Reward Amount is invalid in the block");
                            break;
                        }

                    }
                    else //not a reward transaction
                    {
                        if (!Transaction.Validate(transaction))
                        { 
                            hasPassedAllChecks = false;
                            Logger.Info("Invalid Transaction");
                            break; 
                        }

                        var expectedWalletBalance = Wallet.CalculateBalance(this.LocalChain, transaction.Input.Address,transaction.Input.Timestamp);
                        if (expectedWalletBalance != transaction.Input.Amount)
                        {
                            hasPassedAllChecks = false;
                            Logger.Info("Invalid Transaction input amount");
                            break;
                        }


                        if (!uniqueTransactions.Add(transaction))
                        {
                            hasPassedAllChecks = false;
                            Logger.Info("Identical Transaction appears more than once in the block.");
                            break;
                        }

                    }

                }
                if (!hasPassedAllChecks)
                    break;
            }
            return hasPassedAllChecks;
        }



        public static bool IsValidChain(ReadOnlyCollection<Block> chain)
        {
            if (chain[0].SerializeObject() != Block.Genesis().SerializeObject())
            {
                Logger.Info("The chain has invalid genesis block.");
                return false;
            }

            for (int i = 1; i < chain.Count; i++)
            {
                var block = chain[i];

                var lastBlock = chain[i - 1];

                if (lastBlock.Hash != block.LastHash)
                {
                    Logger.Info("The chain has broken link.");
                    return false;
                }
                if (block.Hash != Helper.Sha256(block.Timestamp.ToString(), block.LastHash, block.Data.SerializeObject(), block.Nonce.ToString(), block.Difficulty.ToString()))
                {
                    Logger.Info("The chain has invalid block hash.");
                    return false;
                }
                if (Math.Abs(lastBlock.Difficulty - block.Difficulty) > 1)
                {
                    Logger.Info("The chain has block with jumped difficulty.");
                    return false;
                }
            }

            Logger.Info("The chain is valid.");
            return true;
        }

        bool IBlockChain.IsValidChain(ReadOnlyCollection<Block> chain)
        {
            return BlockChain.IsValidChain(chain);
        }



        public bool ReplaceChain(ReadOnlyCollection<Block> chain)
        {
            if (this.localChain.Count >= chain.Count)
            {
                Logger.Info("The incoming chain must be longer.");
                return false;
            }
            if (!BlockChain.IsValidChain(chain))
            {
                Logger.Info("The incoming chain must be valid.");
                return false;
            }
            if (!this.ValidateTransactionData(chain))
            {
                Logger.Info("The incoming chain has invalid transaction.");
                return false;
            }

            Logger.Info("The incoming chain is valid.");
            this.localChain = chain.ToList();
            Logger.Info("Replaced local chain.");
            return true;
        }

    }
}
