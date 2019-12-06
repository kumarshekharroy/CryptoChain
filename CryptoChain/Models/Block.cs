using CryptoChain.Services.Classes;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Models
{
    public class Block
    {
        public long TS { get; private set; }
        public string LastHash { get; set; }
        public string Hash { get; set; }
        public object Data { get; set; }
        public long Nonce { get; set; }
        public int Difficulty { get; set; }





        public Block(long ts, string lastHash, string hash, object data, long nonce, int difficulty)
        {
            this.TS = ts;
            this.LastHash = lastHash;
            this.Hash = hash;
            this.Data = data;
            this.Nonce = nonce;
            this.Difficulty = difficulty;



        }


        public static Block Genesis() => new Block(ts: 0L, data: new object(), hash: "genesis-hash", lastHash: "genesis-last-hash", nonce: 0, difficulty: 3);


        public static Block MineBlock(Block lastBlock, object data)
        {
            using var Clock = new Clock();
            long TS;
            string currentBlockHash;

            var nonce = 0L;
            var difficultString = string.Concat(Enumerable.Repeat("0", lastBlock.Difficulty));

            do
            {
                nonce++;
                TS = Clock.UtcNow.ToTicks();

                currentBlockHash = Helper.Sha256(TS.ToString(), lastBlock.Hash, data.SerializeObject(), nonce.ToString(), lastBlock.Difficulty.ToString());

            } while (!currentBlockHash.StartsWith(difficultString));




            return new Block(ts: TS, lastHash: lastBlock.Hash, data: data, nonce: nonce, difficulty: lastBlock.Difficulty, hash: currentBlockHash);
        }
    }
}
