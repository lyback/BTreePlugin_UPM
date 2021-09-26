
using UnityEngine;
using Newtonsoft.Json;

namespace BTree.Editor
{
    public class BTreeEditorHelper
    {
        public const string EditorResPath = "Assets/Editor/BTreeEditor/Resources/";
        public const string EditorResPath_Package = "Packages/com.centurygame.btree/Editor/BTreeEditor/Resources/";
        public const string EditorConfigPath = "Assets/Editor/BTreeEditor/Config/";
        public const string LuaConfigPath = "Assets/Lua/config/btree/";
        public const string PreconditionPath = "Assets/RunTime/BTree/Precondition/";
        public const string PreconditionBasePath = "Assets/Editor/BTree/BTreeFrame/BTreeNodePrecondition/";
        public static readonly string[] PreconditionLabel = new string[1] { "Btree_Precondition" };

        public static void WirteFileAtPath(string str, string _path)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(_path);
            file.Write(str);
            file.Close();
        }
        public static string ReadFileAtPath(string _path)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(_path);
            string str = file.ReadToEnd();
            file.Close();
            return str;
        }
        public static string ToJson(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
            };
            string json = JsonConvert.SerializeObject(obj, settings);
            Debug.Log(json);
            return json;
        }
        public static T FromJson<T>(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new BTreeNodeConverter(), new BTreeNodePreconditionConverter() },
                NullValueHandling = NullValueHandling.Ignore,
            };
            T res;
            try
            {
                res = JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (System.Exception)
            {
                Debug.Log("BTREE ERROR:" + json);
                throw;
            }
            return res;
        }
    }
}
