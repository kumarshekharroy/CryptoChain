using CryptoChain.Models;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace CryptoChain.Services.Interfaces
{
    public interface IRedis
    {
       // ConnectionMultiplexer Connection { get; }
        IDatabase DB0 { get; }
        //ISubscriber Subscriber { get; }

        IDatabase GetDB(int db = 0);
        //void PublishToChannel(string channelName, object message);
        //Task PublishToChannelAsync(string channelName, object message);
        Task BroadcastChain();
        Task BroadcastTransaction(Transaction transaction);
    }
}