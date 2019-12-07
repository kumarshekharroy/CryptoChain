using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        }


        public ReadOnlyCollection<Block> LocalChain { get { return this.localChain.AsReadOnly(); } }

        public void AddBlock(object data)
        {
            var newBlock = Block.MineBlock(localChain[localChain.Count - 1], data);

            this.localChain.Add(newBlock);
        }




        public static bool IsValidChain(List<Block> chain)
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
                if (block.Hash != Helper.Sha256(block.Timestamp.ToString(), block.LastHash, block.Data.SerializeObject()))
                {
                    Logger.Info("The chain has invalid block.");
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

        bool IBlockChain.IsValidChain(List<Block> chain)
        {
            return BlockChain.IsValidChain(chain);
        }



        public void ReplaceChain(List<Block> chain)
        {
            if (this.localChain.Count >= chain.Count)
            {
                Logger.Info("The incoming chain must be longer.");
                return;
            }
            if (!BlockChain.IsValidChain(chain))
            {
                Logger.Info("The incoming chain must be valid.");
                return;
            }

            Logger.Info("The incoming chain is valid.");
            this.localChain = chain;
            Logger.Info("Replaced local chain.");
        }

    }
}
