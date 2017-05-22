using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ReferenceConflictAnalyser.DataStructures;

namespace ReferenceConflictAnalyser.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new ReferenceReader();
            var entryAssemblyPath = args[0];
            string configFilePath;
            ConfigurationHelper.TrySuggestConfigFile(entryAssemblyPath, out configFilePath);

            var result = reader.Read(entryAssemblyPath, configFilePath);


            Console.WriteLine("Select mode: C - outout to console, D - output to dgml file");
            var mode = Console.ReadKey();

            switch (mode.Key)
            {
                case ConsoleKey.C:
                    WriteReferencesToConsole(result);
                    break;

                case ConsoleKey.D:
                    WriteReferencesToDgmlFile(result);
                    break;

            }
        }

        private static void WriteReferencesToDgmlFile(ReferenceList result)
        {
            var builder = new GraphBuilder();
            var doc = builder.BuildDgml(result);

            var path = Path.Combine(Path.GetTempFileName() + ".dgml");
            doc.Save(path);

            Process.Start(path);
        }

        private static void WriteReferencesToConsole(ReferenceList result)
        {
            Console.WriteLine("References:");
            foreach (var reference in result.References)
                Console.WriteLine(reference);

            Console.WriteLine();
            Console.WriteLine("Errors:");
            foreach (var err in result.LoadingErrors)
                Console.WriteLine(err);

            Console.ReadKey();
        }

 
    }
}
