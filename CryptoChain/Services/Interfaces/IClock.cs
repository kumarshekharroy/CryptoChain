using System;

namespace CryptoChain.Services.Interfaces
{
    public interface IClock
    {
        bool IsPrecise { get; }
        DateTime UtcNow { get; }
    }
}