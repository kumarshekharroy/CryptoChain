using CryptoChain.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CryptoChain.Services.Interfaces
{
    public interface IBlockChain
    {
        void AddBlock(List<Transaction> data);
        void ReplaceChain(List<Block> chain);
        bool IsValidChain(List<Block> chain);
        ReadOnlyCollection<Block> LocalChain { get; }
    }
}