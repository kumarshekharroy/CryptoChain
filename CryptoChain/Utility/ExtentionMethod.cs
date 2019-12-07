using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoChain.Utility
{
    public static class ExtentionMethod
    {

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public static ConcurrentDictionary<TKey, TValue>? ToConcurrentDictionary<TKey, TValue>(this Dictionary<TKey, TValue>? dict)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            if (dict == null)
                return null;
            return new ConcurrentDictionary<TKey, TValue>(dict);
        }

        public static Dictionary<string, string> ToReleventInfo(this Exception e)
        {
            Dictionary<string, string> error;
            try
            {
                error = new Dictionary<string, string> {
                                {"Type", e.GetType().ToString()},
                                {"Message", e.Message},
                                {"StackTrace", e.StackTrace??string.Empty},
                                {"InnerException", e.InnerException?.Message??string.Empty}
                };


                foreach (DictionaryEntry? data in e.Data)
                    error.Add(data?.Key?.ToString() ?? string.Empty, data?.Value?.ToString() ?? string.Empty);
                return error;
            }
            catch (Exception ex)
            {
                error = new Dictionary<string, string> {
                                {"Message", ex.Message},
                                {"InnerMessage", e.Message}};
            }
            return error;
        }

        public static string SerializeObject(this object myObject, bool isFormattingIntended = true) => JsonConvert.SerializeObject(myObject, isFormattingIntended ? Formatting.Indented : Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });


        public static decimal TruncateAfter(this decimal d, byte decimals)
        {
            decimal r = Math.Round(d, decimals);

            if (d > 0 && r > d)
            {
                return r - new decimal(1, 0, 0, false, decimals);
            }
            else if (d < 0 && r < d)
            {
                return r + new decimal(1, 0, 0, false, decimals);
            }
            return r;
        }

        public static long ToLong(this double d) => Convert.ToInt64(Math.Truncate(d));


        public static string? GetIP(this HttpContext httpContext) => httpContext.Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

        #region DateTime
        public static readonly DateTime MinDateTimeJS = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static long ToSec(this DateTime dt) => (dt - MinDateTimeJS).TotalSeconds.ToLong();

        public static long ToSecIgnoreTime(this DateTime dt) => (dt.Date - MinDateTimeJS).TotalSeconds.ToLong();

        public static long ToMilliSec(this DateTime dt) => (dt - MinDateTimeJS).TotalMilliseconds.ToLong();

        public static long ToMilliSecIgnoreTime(this DateTime dt) => (dt.Date - MinDateTimeJS).TotalMilliseconds.ToLong();

        public static long ToTicks(this DateTime dt) => (dt.Ticks - MinDateTimeJS.Ticks);

        public static long ToTicksIgnoreTime(this DateTime dt) => (dt.Date.Ticks - MinDateTimeJS.Ticks);


        public static string ToTicksHexString(this DateTime dt) => dt.ToTicks().ToString("X");
        public static string ToTicksHexStringIgnoreTime(this DateTime dt) => dt.Date.ToTicks().ToString("X");



        public static DateTime MilliSecToDateTime(this long millisec) => MinDateTimeJS.AddMilliseconds(millisec);
        public static DateTime SecToDateTime(this long sec) => MinDateTimeJS.AddSeconds(sec);
        public static DateTime TicksToDateTime(this long ticks) => MinDateTimeJS.AddTicks(ticks);

        public static DateTime TicksHexStringToDateTime(string ticksHexString) => TicksHexStringToTicks(ticksHexString).TicksToDateTime();
        public static long TicksHexStringToTicks(string ticksHexString)
        {
            try
            {
                return Convert.ToInt64(ticksHexString, 16);
            }
            catch// (Exception ex)
            {
                return 0L;
            }
        }
        #endregion




    }
}
