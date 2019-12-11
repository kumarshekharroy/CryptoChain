using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoChain.Models;
using CryptoChain.Services.Interfaces;
using CryptoChain.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CryptoChain.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private static readonly bool IsDebugging = !Debugger.IsAttached;

        private readonly ILogger<ApiController> _ILogger;
        private readonly IClock _IClock;
        private readonly IBlockChain _IBlockChain;
        private readonly IRedis _IRedis;
        private readonly IWallet _IWallet;
        private readonly ITransactionPool _ITransactionPool;
        private readonly ITransactionMiner _ITransactionMiner; 


        public ApiController(ILogger<ApiController> logger, IClock clock, IBlockChain blockChain, IRedis redis, IWallet wallet, ITransactionPool transactionPool, ITransactionMiner transactionMiner)
        {
            this._ILogger = logger;
            this._IClock = clock;
            this._IBlockChain = blockChain;
            this._IRedis = redis;
            this._IWallet = wallet;
            this._ITransactionPool = transactionPool;
            this._ITransactionMiner = transactionMiner;
        }
        [HttpPost, HttpGet]
        [Route("~/ping")]
        public IActionResult ping()
        {

            _ILogger.LogDebug($"~/ping requested from {HttpContext.GetIP()}");
            return StatusCode(StatusCodes.Status200OK, "pong");
        }

        [HttpGet]
        [Route("blocks")]
        public IActionResult blocks()
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, _IBlockChain.LocalChain);
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/blocks requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.QueryString.Value}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }

        [HttpPost]
        [Route("mine")]
        public async Task<IActionResult> mine()
        {
            try
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
                using var reader = new StreamReader(Request.Body);
                var parsed_body = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());

                var data = parsed_body.data;

                _IBlockChain.AddBlock(data);
                await _IRedis.BroadcastChain();
                return Redirect("/api/blocks");
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/mine requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.Body}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }
        [HttpPost]
        [Route("transact")]
        public async Task<IActionResult> transact()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var parsed_body = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());

                Transaction transaction;
                try
                {
                    string recipient = parsed_body.recipient?.ToString() ?? throw new ArgumentException("Invalid recipient supplied.");

                    if (!long.TryParse(parsed_body.amount?.ToString(), out long amount)) throw new ArgumentException("Invalid recipient supplied.");

                    transaction = _ITransactionPool.ExistingTransaction(_IWallet.PublicKey);

                    if (transaction == null)
                        transaction = _IWallet.CreateTransaction(recipient, amount,_IBlockChain.LocalChain);
                    else
                        transaction.Update(_IWallet, recipient, amount);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiResponse { message = ex.Message });
                }

                _ITransactionPool.SetTransaction(transaction);

                await _IRedis.BroadcastTransaction(transaction);

                return StatusCode(StatusCodes.Status200OK, transaction);
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/transact requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.Body}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }

        [HttpGet]
        [Route("transaction-pool")]
        public IActionResult transactionpool()
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, _ITransactionPool.TransactionMap);
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/transaction-pool requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.QueryString.Value}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }
        [HttpGet]
        [Route("wallet-info")]
        public IActionResult walletinfo()
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, new { Address=_IWallet.PublicKey,Balance= Services.Classes.Wallet.CalculateBalance(_IBlockChain.LocalChain,_IWallet.PublicKey,_IClock.UtcNow)});
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/wallet-info requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.QueryString.Value}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }

        [HttpGet]
        [Route("mine-transactions")]
        public async Task<IActionResult> minetransactions()
        {
            try
            { 
                await _ITransactionMiner.MineTransaction();
                return Redirect("/api/blocks");
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/mine-transactions requested from {HttpContext.GetIP()}.");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }

    }
}