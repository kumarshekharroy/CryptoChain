using CryptoChain.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoChain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CryptoChain.Utility;

namespace CryptoChain.Services.Classes
{
    public class Redis : Interfaces.IRedis
    {
        private readonly Lazy<ConnectionMultiplexer> LazyConnection;

        private readonly ILogger<Redis> _ILogger;
        // private readonly IClock _IClock;
        private readonly IBlockChain _IBlockChain;

        public Redis(ILogger<Redis> logger, IBlockChain blockChain)//, IClock clock)
        {
            this._ILogger = logger;
            // this._IClock = clock;
            this._IBlockChain = blockChain;
            ConfigurationOptions options = new ConfigurationOptions()
            {
                ClientName = Constants.APP_NANE,
                SyncTimeout = Constants.REDIS_TIMEOUT_IN_MILLISEC,
                AbortOnConnectFail = Constants.REDIS_ABORT_ON_CONNECT_FAIL,
                ConnectRetry = Constants.REDIS_CONNECT_RETRY_LIMIT,
                EndPoints = { { Constants.REDIS_SERVER, Constants.REDIS_PORT } },
                ReconnectRetryPolicy = new ExponentialRetry(5000),
                Password = Constants.REDIS_PASSWORD
                //,
                //DefaultDatabase = 0,
                //Ssl = true,
            };
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));

            SubscribeToChannels();
        }
        private ConnectionMultiplexer Connection => LazyConnection.Value;

        private ISubscriber Subscriber => Connection.GetSubscriber();

        public IDatabase DB0 => Connection.GetDatabase(0);
        public IDatabase GetDB(int db = 0) => Connection.GetDatabase(db);



        private void PublishToChannel(string channelName, string message)
        {
            this.Subscriber.Publish(channelName, (RedisValue)message);
        }

        private async Task PublishToChannelAsync(string channelName, string message)
        {
            await this.Subscriber.PublishAsync(string.Concat(Constants.PUBSUB_CHANNEL_PPREFIX,"-", channelName) , message);
        }


        private void SubscribeToChannels()
        {
            foreach (var channelInfo in typeof(Constants.PUBSUB_CHANNEL).GetProperties())
            {
                _ILogger.LogInformation($"Subscribing to Channel : {channelInfo.Name}");

                this.Subscriber.Subscribe(string.Concat("*","-",channelInfo.Name)).OnMessage(channelMessage =>
                {
                    //Note: exceptions are caught and discarded by StackExchange.Redis here, to prevent cascading failures. To handle failures, use a try/catch inside your handler to do as you wish with any exceptions.
                    _ILogger.LogInformation($"Channel : {(string)channelMessage.Channel} => {(string)channelMessage.Message} ");

                    //Discard Self published message
                    if (((string)channelMessage.Channel).StartsWith(Constants.PUBSUB_CHANNEL_PPREFIX))
                    {
                        _ILogger.LogInformation($"Discarded Self Published Message from Channel : {(string)channelMessage.Channel}"); 
                    }
                    _ILogger.LogInformation($"Channel : {(string)channelMessage.Channel} => {(string)channelMessage.Message} ");

                    switch ((string)channelMessage.Channel)
                    {

                        case "BLOCKCHAIN":
                            try
                            {
                                var newChain = JsonConvert.DeserializeObject<List<Block>>(channelMessage.Message);
                                this._IBlockChain.ReplaceChain(newChain);

                            }
                            catch (Exception ex)
                            {
                                _ILogger.LogError(ex, $"Error while processing a message from Channel : BLOCKCHAIN");
                            }
                            break;

                        default:
                            _ILogger.LogInformation($"Discarded a message from Channel : {(string)channelMessage.Channel}");
                            break;
                    }

                });
            }
        }


        public async Task BroadcastChain()
        {
            await this.PublishToChannelAsync(Constants.PUBSUB_CHANNEL.BLOCKCHAIN, this._IBlockChain.LocalChain.SerializeObject());
        }
    }
}
