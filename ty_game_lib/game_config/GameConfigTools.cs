using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace game_config
{
    public static class GameConfigTools
    {
        private static string GetNameSpace()
        {
            var declaringType = System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType;
            return declaringType != null ? declaringType.Namespace : "";
        }

        private static readonly string DllName = GetNameSpace() + ".dll";
        private static readonly string ResLocate = GetNameSpace()+ ".Resource.";


        public static ImmutableDictionary<int, T> GenConfigDict<T>()
        {
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(T), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(T));
            var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), DllName);
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var stream = assembly.GetManifestResourceStream(ResLocate + name);
            using var reader =
                new StreamReader(stream ?? throw new Exception("NoResource" + name), Encoding.UTF8);
            var json = reader.ReadToEnd();
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(json);
            var jToken = deserializeObject["content"];
            var genConfigDict = jToken?.ToObject<ImmutableDictionary<int, T>>();
            return genConfigDict;
        }
    }

    public interface IGameConfig
    {
    }
}