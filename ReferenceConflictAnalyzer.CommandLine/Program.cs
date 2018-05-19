using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyzer.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args == null
                    || !args.Any()
                    || args[0] == "?"
                    || args[0] == "help")
                {
                    Console.WriteLine(@"Parameters: 

 -file - (mandatory) path to assembly (DLL or executable) to analyze;
 -config - (optional) path to related config file;
 -ignoreSystemAssemblies - (optional, default value is 'true') exclude from the analysis assemblies with names starting with 'System';
 -output - (mandatory) directory to put genegated DGML file to.");
                    Console.ReadKey();
                }
                else
                    new GenerateDgmlFileCommand().Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
            }
        }

        
    }
}
