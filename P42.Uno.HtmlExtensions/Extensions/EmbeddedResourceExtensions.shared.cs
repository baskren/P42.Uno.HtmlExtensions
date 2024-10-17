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
        public static async Task<StorageFile> ResourceAsStorageFile(string resourceId, Assembly asm = null)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                throw new ArgumentException(nameof(resourceId));

            asm = asm ??  FindAssemblyForResource(resourceId);

            if (asm.GetManifestResourceNames().FirstOrDefault(name => name.EndsWith(resourceId)) is not String resourceName)
                return null;

            await using var inStream = asm.GetManifestResourceStream(resourceName);
            /*
                    using (var fileStream = File.Create(Path.Combine(Windows.Storage.ApplicationData.Current.TemporaryFolder.Path, Path.GetRandomFileName() + ".html")))
                    {
                        inStream.Seek(0, SeekOrigin.Begin);
                        inStream.CopyTo(fileStream);
                    }

                    var x = await Windows.Storage.ApplicationData.Current.LocalCacheFolder.CreateFileAsync("test.txt");
                    */

            var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //var tempFolder = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            var folder = await localFolder.CreateFolderAsync("P42.Uno.WebViewResource", CreationCollisionOption.OpenIfExists);

            var file = await folder.CreateFileAsync(Path.GetRandomFileName() + ".html");
            await using var outStream = File.OpenWrite(file.Path);
            await inStream.CopyToAsync(outStream);
            return file;
        }


        static readonly Dictionary<Assembly, string[]> _embeddedResourceNames = new Dictionary<Assembly, string[]>();

        /// <summary>
        /// Finds the assembly that contains an embedded resource matching the resourceId
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static Assembly FindAssemblyForResource(string resourceId, Assembly assembly = null)
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
                if (_embeddedResourceNames.TryGetValue(assembly, out string[] names))
                    return names.Any(n=>n.EndsWith(resourceId))  ? assembly : null;
                names = assembly.GetManifestResourceNames();
                if (names != null)
                {
                    _embeddedResourceNames[assembly] = names;
                    return names.Any(n => n.EndsWith(resourceId)) ? assembly : null;
                }
            }
            else
            {

                if (_embeddedResourceNames.TryGetValue(assembly, out string[] names))
                    return names.Contains(resourceId) ? assembly : null;
                names = assembly.GetManifestResourceNames();
                if (names != null)
                {
                    _embeddedResourceNames[assembly] = names;
                    return names.Contains(resourceId) ? assembly : null;
                }
            }
            return null;
        }

        static List<Assembly> GetAssemblies()
        {
            var currentDomain = System.AppDomain.CurrentDomain;
            var assemblies = currentDomain.GetAssemblies();
            var result = assemblies?.ToList(); //new List<Assembly>(assemblies);
            return result;
        }

        static Assembly GetAssemblyByName(string name)
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
