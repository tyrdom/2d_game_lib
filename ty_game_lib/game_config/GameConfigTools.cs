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

#if UNITY_2018_1_OR_NEWER
using UnityEngine;
#endif

namespace game_config
{
    public static class GameConfigTools
    {

        private static readonly char Sep = Path.DirectorySeparatorChar;
#if UNITY_2018_1_OR_NEWER
#else
        //二进制文件存取位置
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

        private static void SaveDictByByte<TK, TV>(ImmutableDictionary<TK, TV> dictionary, FileInfo fileNameLocation)
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

        private static ImmutableDictionary<TK, TV> LoadDictByByte<TK, TV>(FileInfo fileInfo)
        {
            var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using var binaryFile = fileInfo.OpenRead();
            var readBack = binaryFormatter.Deserialize(binaryFile) as Dictionary<TK, TV>;
            binaryFile.Flush();
            Console.Out.WriteLine($"{readBack}");
            if (readBack != null) return CovertDictToIDict(readBack);
            throw new Exception("ErrorResource:::" + fileInfo);
        }

        private static string GetNameSpace()
        {
            var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
            return declaringType != null ? declaringType.Namespace : "";
        }

        public static readonly string DllName = GetNameSpace() + ".dll";
        public static readonly string ResLocate = GetNameSpace() + ".Resources.";
#endif
#if NETCOREAPP
        public static ImmutableDictionary<TK, TV> GenConfigDict<TK, TV>()
        {
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(TV), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(TV));
            var assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), DllName);
            var assembly =
                AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

            var stream =
                assembly.GetManifestResourceStream(ResLocate + name);
            using var reader =
                new StreamReader(stream ?? throw new Exception("NoResource:::" + name), Encoding.UTF8);
            var json = reader.ReadToEnd();
            return GenConfigDictByJsonString<TK, TV>(json);
        }
#endif
        public static ImmutableDictionary<TK, TV> GenConfigDictByJsonString<TK, TV>(string jsonSting)
        {
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(jsonSting);
            var jToken = deserializeObject["content"];

            var genConfigDict = jToken?.ToObject<ImmutableDictionary<TK, TV>>();
            if (genConfigDict != null) return genConfigDict;
            throw new Exception("ErrorResource:::" + jsonSting);
        }


#if UNITY_2018_1_OR_NEWER
        public static string JsonRead(string path, string name)
        {
            string json = "";
            var replace = name.Replace(".json", "");
            var sep = path + Sep + replace;
            Debug.Log(sep);
            TextAsset text = Resources.Load<TextAsset>(sep);
            json = text.text;
            if (string.IsNullOrEmpty(json)) return "";
            return json;
        }

        public static ImmutableDictionary<TK, TV> GenConfigDictByJsonFile<TK, TV>(string jsonPath = "")
        {
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(TV), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(TV));


            var json = JsonRead(jsonPath, name);

            var deserializeObject = JsonConvert.DeserializeObject<JObject>(json);
            var jToken = deserializeObject["content"];
            var genConfigDict = jToken?.ToObject<ImmutableDictionary<TK, TV>>();
            if (genConfigDict != null) return genConfigDict;
            throw new Exception("ErrorResource:::" + name);
        }
#else


        public static ImmutableDictionary<TK, TV> GenConfigDictByJsonFile<TK, TV>(string jsonPath = "")
        {
            if (jsonPath == "")
                return new Dictionary<TK, TV>().ToImmutableDictionary();
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(TV), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(TV));
            var strings = FindFile(jsonPath, name);
            var s = strings;
            Console.Out.WriteLine($"aaa{s} !{jsonPath} !{name}");
            var json = File.ReadAllText(s, Encoding.UTF8);
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(json);
            var jToken = deserializeObject["content"];
            var genConfigDict = jToken?.ToObject<ImmutableDictionary<TK, TV>>();
            return genConfigDict ?? new Dictionary<TK, TV>().ToImmutableDictionary();
        }

        private static string FindFile(string path, string name)
        {
            var strings = Directory.GetFiles(path, name);
            if (strings.Length > 0)
            {
                return strings[0];
            }

            var directories = Directory.GetDirectories(path);

            return directories.Length > 0
                ? directories.Select(directory => FindFile(directory, name)).FirstOrDefault(s => s != "")
                : "";
        }
#endif
    }

    public interface IGameConfig
    {
    }
}