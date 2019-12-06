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

        public Block(long ts,string lastHash,string hash,object data)
        {
            this.TS = ts;
            this.LastHash = lastHash;
            this.Hash = hash;
            this.Data = data;


        }


        public static Block Genesis()=> new Block ( ts : 0L, data : new object(), hash : "genesis-hash", lastHash : "genesis-last-hash" );
    

        public static Block MineBlock(Block lastBlock,object data)
        {
            using var Clock = new Clock();
            var TS = Clock.UtcNow.ToTicks();

           return new Block(ts: TS, lastHash: lastBlock.Hash, data: data, hash: Helper.Sha256(TS.ToString(), lastBlock.Hash, data.SerializeObject()));
        }
    }
}
