﻿
using SlurperDotNetCore.Contracts;
using SlurperDotNetCore.Output;

namespace SlurperDotNetCore.Providers
{
   public static class LogProvider
    {
        public static ILogger Logger { get; } = new ConsoleLogger();
    }
}
