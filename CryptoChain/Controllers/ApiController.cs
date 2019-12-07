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


        public ApiController(ILogger<ApiController> logger, IClock clock, IBlockChain blockChain)
        {
            this._ILogger = logger;
            this._IClock = clock;
            this._IBlockChain = blockChain;

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
                using var reader = new StreamReader(Request.Body); 
                var parsed_body = JsonConvert.DeserializeObject<dynamic>(await reader.ReadToEndAsync());

                var data = parsed_body.data;

                _IBlockChain.AddBlock(data);

                return Redirect("/api/blocks");
            }
            catch (Exception ex)
            {
                _ILogger.LogError(ex, $"~/api/mine requested from {HttpContext.GetIP()}. Payload : { HttpContext.Request.QueryString.Value}");

                if (IsDebugging)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message, data = ex.ToReleventInfo() });
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { message = ex.Message });
            }

        }

    }
}