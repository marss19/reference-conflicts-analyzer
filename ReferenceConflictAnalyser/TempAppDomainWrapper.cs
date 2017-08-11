using ReferenceConflictAnalyser.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReferenceConflictAnalyser
{
    public class TempAppDomainWrapper
    {
        public static string AnalyzeAssemblies(string entryAssemblyFilePath, string configPath, bool skipSystemAssemblies = true)
        {
            var info = new AppDomainSetup()
            {
                ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            AppDomain tempDomain = null;
            try
            {
                tempDomain = AppDomain.CreateDomain("TempAppDomain", null, info);

                tempDomain.SetData(EntryAssemblyFilePathKey, entryAssemblyFilePath);
                tempDomain.SetData(SkipSystemAssembliesKey, skipSystemAssemblies);
                tempDomain.SetData(ConfigPathKey, configPath);

                tempDomain.DoCallBack(DoWork);

                var graphDgml = tempDomain.GetData(GraphKey);

                return (string)graphDgml;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                AppDomain.Unload(tempDomain);
            }
        }

        #region private

        private const string EntryAssemblyFilePathKey = "EntryAssemblyFilePath";
        private const string SkipSystemAssembliesKey = "SkipSystemAssemblies";
        private const string ConfigPathKey = "configPath";
        private const string GraphKey = "GraphKey";

        private static void DoWork()
        {
            var entryAssemblyFilePath = AppDomain.CurrentDomain.GetData(EntryAssemblyFilePathKey).ToString();
            var configPath = AppDomain.CurrentDomain.GetData(ConfigPathKey).ToString();
            var skipSystemAssemblies = (bool)AppDomain.CurrentDomain.GetData(SkipSystemAssembliesKey);

            var reader = new ReferenceReader();
            var result = reader.Read(entryAssemblyFilePath, skipSystemAssemblies);

            var bindingRedirects = ConfigurationHelper.GetBindingRedirects(configPath);

            var analyser = new ReferenceAnalyser();
            result = analyser.AnalyzeReferences(result, bindingRedirects);

            var builder = new GraphBuilder();
            var doc = builder.BuildDgml(result);

            AppDomain.CurrentDomain.SetData(GraphKey, doc.OuterXml);
        }

        #endregion

    }
}
