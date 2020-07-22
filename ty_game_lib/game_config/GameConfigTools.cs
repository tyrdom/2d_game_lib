using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace game_config
{
    public static class GameConfigTools
    {
        //二进制文件存取位置
        private static readonly char Sep = Path.DirectorySeparatorChar;
        private static readonly string ByteDir = $".{Sep}Bytes{Sep}";

        public static Dictionary<TK, TV> CovertIDictToDict<TK, TV>(ImmutableDictionary<TK, TV> immutableDictionary)
        {
            return immutableDictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static ImmutableDictionary<TK, TV> CovertDictToIDict<TK, TV>(Dictionary<TK, TV> immutableDictionary)
        {
            return immutableDictionary.ToImmutableDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static FileInfo GenFileInfo<T>()
        {
            var s = typeof(T).ToString();

            return new FileInfo($".{ByteDir}{s}.bytes");
        }

        public static void SaveDict<TK, TV>(ImmutableDictionary<TK, TV> dictionary)
        {
            var fileNameLocation = GenFileInfo<TV>();
            if (fileNameLocation.Directory != null && !fileNameLocation.Directory.Exists)
                fileNameLocation.Directory.Create();

            SaveDictByByte(dictionary, fileNameLocation);
        }

        public static void SaveDictByByte<TK, TV>(ImmutableDictionary<TK, TV> dictionary, FileInfo fileNameLocation)
        {
            var vs = dictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using var binaryFile = fileNameLocation.Create();
            binaryFormatter.Serialize(binaryFile, vs);
            binaryFile.Flush();
        }

        public static ImmutableDictionary<TK, TV> LoadDict<TK, TV>()
        {
            return LoadDictByByte<TK, TV>(GenFileInfo<TV>());
        }

        public static ImmutableDictionary<TK, TV> LoadDictByByte<TK, TV>(FileInfo fileInfo)
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using var binaryFile = fileInfo.OpenRead();
            var readBack = binaryFormatter.Deserialize(binaryFile) as Dictionary<TK, TV>;
            binaryFile.Flush();
            Console.Out.WriteLine($"{readBack}");
            return CovertDictToIDict(readBack);
        }

        private static string GetNameSpace()
        {
            var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
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
            // Console.Out.WriteLine($"{typeof(TK)}==={typeof(T)}");
            var genConfigDict = jToken?.ToObject<ImmutableDictionary<TK, T>>();
            return genConfigDict;
        }
    }


    public interface IGameConfig
    {
    }
}