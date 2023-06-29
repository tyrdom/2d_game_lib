using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
#if NETCOREAPP
using System.Reflection;
using System.Runtime.Loader;
#endif

using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if UNITY_2018_1_OR_NEWER
using UnityEngine;
#endif


namespace game_config
{
    public interface IShowSchemeConfig
    {
        string Name { get; set; }
        string icon { get; set; }

        string Describe { get; set; }
    }
    public interface IBodyActionConfig
    {
     
    }
    public static class GameConfigTools


    {
        private static readonly char Sep = Path.DirectorySeparatorChar;

        //二进制文件存取

        public static string SerializeObj<T>(T obj)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, obj);
                stream.Position = 0;
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Flush();
                stream.Close();
                return Convert.ToBase64String(buffer);
            }
            catch (Exception ex)
            {
                throw new Exception("序列化失败,原因:" + ex.Message);
            }
        }

        /// <summary>
        /// 反序列化 字符串到对象
        /// </summary>
        /// <param name="str">要转换为对象的字符串</param>
        /// <param name="obj">泛型对象</param>
        /// <returns>反序列化出来的对象</returns>
        public static T DesObj<T>(string str, T obj)
        {
            try
            {
                obj = default(T);
                IFormatter formatter = new BinaryFormatter();
                var buffer = Convert.FromBase64String(str);
                var stream = new MemoryStream(buffer);
                obj = (T)formatter.Deserialize(stream);
                stream.Flush();
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("反序列化失败,原因:" + ex.Message);
            }

            return obj;
        }

#if NETCOREAPP
        private static string GetNameSpace()
        {
            var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType;
            return declaringType?.Namespace ?? "";
        }

        public static readonly string DllName = GetNameSpace() + ".dll";
        public static readonly string ResLocate = GetNameSpace() + ".Resources.";
        public static Dictionary<TK, TV> GenConfigDict<TK, TV>()
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
        public static Dictionary<TK, TV> GenConfigDictByJsonString<TK, TV>(string jsonSting)
        {
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(jsonSting);
            var jToken = deserializeObject["content"];
            // Console.Out.WriteLine($"cov {jToken}");
            var genConfigDict = jToken?.ToObject<Dictionary<TK, TV>>();
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


        public static Dictionary<TK, TV> GenConfigDictByJsonFile<TK, TV>(string jsonPath = "")
        {
            var genConfigDictByJsonFile = new Dictionary<TK, TV>();
            if (jsonPath == "")
                return genConfigDictByJsonFile;
            var namesDictionary = ResNames.NamesDictionary;
            if (!namesDictionary.TryGetValue(typeof(TV), out var name))
                throw new Exception("ErrorTypeOfConfig:" + typeof(TV));
            var file = FindFile(jsonPath, name) ?? "";

#if DEBUG
            Console.Out.WriteLine($"aaa{file} !{jsonPath} !{name}");
#endif
            var json = File.ReadAllText(file, Encoding.UTF8);
            var deserializeObject = JsonConvert.DeserializeObject<JObject>(json);
            var jToken = deserializeObject["content"];
            var genConfigDict = jToken?.ToObject<Dictionary<TK, TV>>();
            return genConfigDict ?? genConfigDictByJsonFile;
        }

        private static string? FindFile(string path, string name)
        {
            var strings = Directory.GetFiles(path, name);
            if (strings.Length > 0)
            {
                return strings[0];
            }

            var directories = Directory.GetDirectories(path);

            return directories.Length > 0
                ? directories.Select(directory => FindFile(directory, name)).FirstOrDefault(s => s != "")
                : null;
        }
#endif
    }

    public interface IGameConfig
    {
    }
}