using CryptoChain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Models
{
    public class BlockChain
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private List<Block> localChain;



        public BlockChain()
        {
            this.localChain = new List<Block> { Block.Genesis() };
        }



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

                var lastBlockHash = chain[i - 1].Hash;

                if (block.LastHash != lastBlockHash)
                {
                    Logger.Info("The chain has broken link.");
                    return false;
                }
                if (block.Hash != Helper.Sha256(block.TS.ToString(), block.LastHash, block.Data.SerializeObject()))
                {
                    Logger.Info("The chain has invalid block.");
                    return false;
                }
            }

            Logger.Info("The chain is valid.");
            return true;
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
