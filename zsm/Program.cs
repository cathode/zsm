using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime;
using System.IO;

namespace zsm
{
    class Program
    {
        static void Main(string[] args)
        {
            var tool = new PurgeTool();
            tool.Run(args);

            //if (args.Length > 0)
            //{
            //    var toolcmd = args[1];

            //    switch (toolcmd.ToLower())
            //    {
            //        case "purge":
            //            var instance = new PurgeTool();

            //            instance.Run(args);
            //            break;

            //        default:
            //            Console.WriteLine("usage: zsm purge");
            //            break;
            //    }
            //}
        }
    }
}


