using System.Threading.Tasks;

namespace CryptoChain.Services.Interfaces
{
    public interface ITransactionMiner
    {
        Task MineTransaction();
    }
}