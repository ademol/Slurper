using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper.Providers
{
   public static class LogProvider
    {
        public static ILogger Logger { get; } = new ConsoleLogger();
    }
}
