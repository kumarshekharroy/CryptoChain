using CryptoChain.Services.Classes; 
using CryptoChain.Utility;
using System; 
using System.Linq; 

namespace CryptoChain.Models
{
    public class Block
    { 
        public string LastHash { get; set; }
        public object Data { get; set; }
        public long Nonce { get; set; }
        public int Difficulty { get; set; }
        public DateTime Timestamp { get; private set; }
        public string Hash { get; set; }





      private Block(DateTime timestamp, string lastHash, string hash, object data, long nonce, int difficulty)
        {
            this.Timestamp = timestamp;
            this.LastHash = lastHash;
            this.Hash = hash;
            this.Data = data;
            this.Nonce = nonce;
            this.Difficulty = difficulty;



        }


        public static Block Genesis() => new Block(timestamp: Config.GENESIS_TIMESTAMP, data: Config.GENESIS_DATA, lastHash: Config.GENESIS_PREV_HASH, nonce: Config.GENESIS_NONCE, difficulty: Config.GENESIS_DIFFICULTY, hash: Helper.Sha256(Config.GENESIS_TIMESTAMP.ToString(), Config.GENESIS_PREV_HASH, Config.GENESIS_DATA.SerializeObject(), Config.GENESIS_NONCE.ToString(), Config.GENESIS_DIFFICULTY.ToString()));


        public static Block MineBlock(Block lastBlock, object data)
        {
            using var Clock = new Clock();

            DateTime timestamp;
            string currentBlockHash;
            long nonce = 0L;
            int difficulty = lastBlock.Difficulty;

           // var difficultString = string.Concat(Enumerable.Repeat("0", difficulty));

            do
            {
                nonce++;
                timestamp = Clock.UtcNow;
                difficulty = Block.AdjustDifficulty(lastBlock, timestamp); 

                currentBlockHash = Helper.Sha256(timestamp.ToString(), lastBlock.Hash, data.SerializeObject(), nonce.ToString(), difficulty.ToString());

            } while (!Helper.Hex2Binary(currentBlockHash).StartsWith(string.Concat(Enumerable.Repeat("0", difficulty))));




            return new Block(timestamp: timestamp, lastHash: lastBlock.Hash, data: data, nonce: nonce, difficulty: difficulty, hash: currentBlockHash);
        }


        public static int AdjustDifficulty(Block originalBlock,DateTime timestamp)
        {
            if (originalBlock.Difficulty <= 1)
                return 1;

            if ((timestamp - originalBlock.Timestamp).TotalMilliseconds > Config.MINE_RATE_IN_MILLISEC) return originalBlock.Difficulty - 1;

            return originalBlock.Difficulty + 1;
        }
    }
}
