using ReferenceConflictAnalyser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyzer.CommandLine
{
    class GenerateDgmlFileCommand
    {
        public void Run(string[] args)
        {
            ValidateParameters(args);

            var dgml = Workflow.CreateDependenciesGraph(_filePath, _configPath, _ignoreSystemAssemblies);
            var outputFileName = Path.Combine(_outputFolder, $"{DateTime.Now.ToString("dd-MM-yyyy hh-MM-ss")}.dgml");
            File.WriteAllText(outputFileName, dgml);
        }

        #region private 

        private const string ParameterPattern = "^-(?<name>\\w+)=(?<value>.*)$";
        private string _filePath;
        private string _configPath;
        private bool _ignoreSystemAssemblies = true;
        private string _outputFolder;


        private void ValidateParameters(string[] args)
        {
            var re = new Regex(ParameterPattern, RegexOptions.IgnoreCase);

            foreach (var arg in args)
            {
                var match = re.Match(arg);
                if (match.Success)
                {
                    var value = match.Groups["value"].Value;
                    switch (match.Groups["name"].Value)
                    {
                        case "file":
                            _filePath = value;
                            break;

                        case "config":
                            _configPath = value;
                            break;

                        case "ignoreSystemAssemblies":
                            bool t;
                            if (bool.TryParse(value, out t))
                                _ignoreSystemAssemblies = t;
                            else
                                throw new ArgumentException("Incorrect value of 'ignoreSystemAssemblies' parameter");
                            break;

                        case "output":
                            _outputFolder = value;
                            break;
                    }

                }
            }

            if (string.IsNullOrEmpty(_filePath))
                throw new Exception("'file' parameter is mandatory.");

            if (string.IsNullOrEmpty(_outputFolder))
                throw new Exception("'output' parameter is mandatory.");

        }

        #endregion
    }
}
