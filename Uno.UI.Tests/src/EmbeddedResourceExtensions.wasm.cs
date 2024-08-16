using System.Reflection;

namespace Uno.UI.Tests;

internal static class EmbeddedResourceExtensions
{
    private static readonly Dictionary<Assembly, string[]> EmbeddedResourceNames = new();
        
    private static Assembly? FindAssemblyForResource(string? resourceId, Assembly? assembly = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            return null;

        if (assembly==null)
        {
            if (resourceId[0] != '.')
            {
                if (resourceId.IndexOf(".Resources.", StringComparison.Ordinal) is var index and > 0)
                {
                    var assemblyName = resourceId[..index];
                    if (GetAssemblyByName(assemblyName) is {} asmA
                        && FindAssemblyForResource(resourceId, asmA) is not null)
                        return asmA;
                }
            }
            assembly = Application.Current.GetType().Assembly;
            return FindAssemblyForResource(resourceId, assembly) is not null 
                ? assembly 
                : GetAssemblies().FirstOrDefault(asmB => !asmB.IsDynamic && FindAssemblyForResource(resourceId, asmB) is not null);
        }
            
        if (resourceId[0] == '.')
        {
            if (EmbeddedResourceNames.TryGetValue(assembly, out var names))
                return names.Any(n=>n.EndsWith(resourceId))  ? assembly : null;
                
            names = assembly.GetManifestResourceNames();
            EmbeddedResourceNames[assembly] = names;
            return names.Any(n => n.EndsWith(resourceId)) ? assembly : null;
        }

        if (EmbeddedResourceNames.TryGetValue(assembly, out var names1))
            return names1.Contains(resourceId) ? assembly : null;
            
        names1 = assembly.GetManifestResourceNames();
        EmbeddedResourceNames[assembly] = names1;
        return names1.Contains(resourceId) ? assembly : null;
    }

    private static List<Assembly> GetAssemblies()
    {
        var currentDomain = AppDomain.CurrentDomain;
        var assemblies = currentDomain.GetAssemblies();
        var result = assemblies.ToList(); 
        return result;
    }

    private static Assembly? GetAssemblyByName(string name)
    {
        var asms = GetAssemblies();
        return asms.FirstOrDefault(asm => asm.GetName().Name == name);
    }

    public static string? ResourceAsString(string resourceId, Assembly? asm = null)
    {
        asm ??= FindAssemblyForResource(resourceId);
        var resourceIds = asm?.GetManifestResourceNames();

        if (resourceIds?.FirstOrDefault(id => id.EndsWith(resourceId)) is not { } webViewBridgeResourceId
            || !string.IsNullOrWhiteSpace(webViewBridgeResourceId))
            return null;
            
        using var stream = asm?.GetManifestResourceStream(webViewBridgeResourceId);
        if (stream is null)
            return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

}
