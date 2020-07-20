using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace game_config
{
    public static class GameConfigTools
    {
        public static void SaveDict()
        {
        }

        public static ImmutableDictionary<TK, TV> LoadDict<TK, TV>(string local)
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            var fi = new FileInfo(@$"{local}");
            ImmutableDictionary<TK, TV> readBack;
            using (var binaryFile = fi.OpenRead())
            {
                readBack = (ImmutableDictionary<TK, TV>) binaryFormatter.Deserialize(binaryFile);
            }

            return readBack;
        }

        private static string GetNameSpace()
        {
            var declaringType = System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType;
            return declaringType != null ? declaringType.Namespace : "";
        }

        private static readonly string DllName = GetNameSpace() + ".dll";
        private static readonly string ResLocate = GetNameSpace() + ".Resource.";


        public static ImmutableDictionary<TK, T> GenConfigDict<TK, T>()
        {
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(T), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(T));
            var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), DllName);
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var stream = assembly.GetManifestResourceStream(ResLocate + name);
            using var reader =
                new StreamReader(stream ?? throw new Exception("NoResource:::" + name), Encoding.UTF8);
            var json = reader.ReadToEnd();
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(json);
            var jToken = deserializeObject["content"];
            var genConfigDict = jToken?.ToObject<ImmutableDictionary<TK, T>>();
            return genConfigDict;
        }
    }


    public interface IGameConfig
    {
    }
}