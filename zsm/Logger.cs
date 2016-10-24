using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zsm
{
    public class Logger
    {
        public static void Write(object message, params object[] args)
        {
            Console.WriteLine(string.Format("[{0:yyyy-MM-dd HH:mm:ss.fff}] {1}", DateTime.Now, message?.ToString()), args);
        }
    }
}
