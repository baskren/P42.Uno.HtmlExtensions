using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace P42.Uno.HtmlExtensions
{
    public static class EmbeddedResourceExtensions
    {
        private static readonly Dictionary<Assembly, string[]> EmbeddedResourceNames = new();

        /// <summary>
        /// Cache an EmbeddedResource as a StorageFile
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="asm"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<StorageFile> ResourceAsStorageFileAsync(string resourceId, Assembly asm = null)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException(nameof(resourceId));

            asm = asm ??  FindAssemblyForResource(resourceId);

            if (asm?.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resourceId)) is not String
                resourceName)
                return null;

            await using var inStream = asm.GetManifestResourceStream(resourceName);

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
            var cacheFolder = Windows.Storage.ApplicationData.Current.LocalCacheFolder;

            var folder = await cacheFolder.CreateFolderAsync("P42.Uno.WebViewResource", CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync(Path.GetRandomFileName() + ".html");
            await using var outStream = File.OpenWrite(file.Path);
            await inStream.CopyToAsync(outStream);
            return file;
        }


        /// <summary>
        /// Finds the assembly that contains an embedded resource matching the resourceId
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static Assembly FindAssemblyForResource(string resourceId, Assembly assembly = null)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                return null;

            if (assembly==null)
            {
                if (resourceId[0] != '.')
                {
                    if (resourceId.IndexOf(".Resources.", StringComparison.Ordinal) is int index && index > 0)
                    {
                        var assemblyName = resourceId.Substring(0, index);
                        if (GetAssemblyByName(assemblyName) is Assembly asmA
                            && FindAssemblyForResource(resourceId, asmA) is Assembly)
                            return asmA;
                    }
                }
                assembly = Application.Current.GetType().Assembly;
                if (FindAssemblyForResource(resourceId, assembly) is Assembly)
                    return assembly;
                foreach (var asmB in GetAssemblies())
                {
                    if (!asmB.IsDynamic && FindAssemblyForResource(resourceId, asmB) is Assembly)
                        return asmB;
                }
            }
            else if (resourceId[0] == '.')
            {
                if (EmbeddedResourceNames.TryGetValue(assembly, out string[] names))
                    return names.Any(n=>n.EndsWith(resourceId))  ? assembly : null;
                names = assembly.GetManifestResourceNames();
                if (names != null)
                {
                    EmbeddedResourceNames[assembly] = names;
                    return names.Any(n => n.EndsWith(resourceId)) ? assembly : null;
                }
            }
            else
            {

                if (EmbeddedResourceNames.TryGetValue(assembly, out string[] names))
                    return names.Contains(resourceId) ? assembly : null;
                names = assembly.GetManifestResourceNames();
                if (names != null)
                {
                    EmbeddedResourceNames[assembly] = names;
                    return names.Contains(resourceId) ? assembly : null;
                }
            }
            return null;
        }

        private static Assembly GetApplicationAssembly()
        => Assembly.GetEntryAssembly();

        private static List<Assembly> GetAssemblies()
        {
            var currentDomain = System.AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();
            var result = assemblies?.ToList(); //new List<Assembly>(assemblies);
            return result;
        }

        private static Assembly GetAssemblyByName(string name)
        {
            var asms = GetAssemblies();
            foreach (var asm in asms)
            {
                if (asm.GetName().Name == name)
                    return asm;
            }
            return null;
        }


    }

}
