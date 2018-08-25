using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slurper.Providers
{
   public class LogProvider
    {
        ILogger _logger = new ConsoleLogger();

        public LogProvider()
        {
        }

        public ILogger GetLog()
        {
            return _logger;
        }
    }
}
