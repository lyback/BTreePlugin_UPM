using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class FileHelper
{
    /// <summary>
    /// 文件是否存在
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>是否存在</returns>
    public static bool IsFileExist(string fullpath)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (fullpath.Contains(Application.streamingAssetsPath))
        {
            fullpath = fullpath.Replace(Application.streamingAssetsPath + "/", "");
            return AndroidHelper.FileHelper.CallStatic<bool>("IsAssetExist", fullpath);;
        }
#endif
        return File.Exists(fullpath);
    }

    public static byte[] ReadBytesFromFile(string path)
    {
        byte[] bytes = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        // 如果是读apk包里的资源,使用Android帮助库加载
        if (path.Contains (Application.streamingAssetsPath)) {
            path = path.Replace (Application.streamingAssetsPath + "/", "");
			bytes = AndroidHelper.FileHelper.CallStatic<byte[]> ("LoadFile", path);
        }
        else
        {
            if (File.Exists(path))
            {
                bytes = File.ReadAllBytes(path);
            }
        }
#else
        if (File.Exists(path))
        {
            bytes = File.ReadAllBytes(path);
        }
#endif
        return bytes;
    }

    //copy assets目录下的文件到指定路径，仅限Android使用
    public static bool CopyAssetsFileTo(string pathSrc, string pathDst)
    {
        bool ret = true;
        CreateDirectoryFromFile(pathDst);

#if UNITY_ANDROID && !UNITY_EDITOR
		pathSrc = pathSrc.Replace (Application.streamingAssetsPath + "/", "");
		ret = AndroidHelper.FileHelper.CallStatic<bool>("CopyFileTo", pathSrc, pathDst);
#else
        if (File.Exists(pathSrc))
        {
            FileHelper.CopyFileTo(pathSrc, pathDst);
            ret = true;
        }
#endif
        return ret;
    }

    public static void CopyFileTo(string pathSource, string pathDest)
    {
        DeleteFile(pathDest);
        CreateDirectoryFromFile(pathDest);
        File.Copy(pathSource, pathDest);
    }

    public static void MoveFileTo(string pathSrc, string pathDst)
    {
        if (!File.Exists(pathSrc))
        {
            return;
        }

        DeleteFile(pathDst);
        CreateDirectoryFromFile(pathDst);

        File.Move(pathSrc, pathDst);
    }

    public static void CreateDirectoryFromFile(string path)
    {
        path = path.Replace("\\", "/");
        int index = path.LastIndexOf("/");
        string dir = path.Substring(0, index);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    public delegate bool CopyFilter(string file);
    public static void CopyDirectory(string sourcePath, string destinationPath, string suffix = "", bool overwrite = true, CopyFilter onFilter = null)
    {
        if (onFilter != null && onFilter(sourcePath))
        {
            return;
        }

        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        foreach (string file in Directory.GetFileSystemEntries(sourcePath))
        {
            if (File.Exists(file))
            {
                FileInfo info = new FileInfo(file);
                if (string.IsNullOrEmpty(suffix) || file.ToLower().EndsWith(suffix.ToLower()))
                {
                    string destName = Path.Combine(destinationPath, info.Name);
                    if (!(onFilter != null && onFilter(file)))
                    {
                        File.Copy(file, destName, overwrite);
                    }
                }
            }

            if (Directory.Exists(file))
            {
                DirectoryInfo info = new DirectoryInfo(file);
                string destName = Path.Combine(destinationPath, info.Name);
                CopyDirectory(file, destName, suffix, overwrite, onFilter);
            }
        }
    }

    public static bool RenameDirectory(string pathSrc, string pathDst)
    {
        if (!Directory.Exists(pathSrc) || Directory.Exists(pathDst))
        {
            return false;
        }

        Directory.Move(pathSrc, pathDst);
        return true;
    }

    public static void DeleteFolder(string dir)
    {
        foreach (string d in Directory.GetFileSystemEntries(dir))
        {
            if (File.Exists(d))
            {
                FileInfo fi = new FileInfo(d);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                {
                    fi.Attributes = FileAttributes.Normal;
                }

                File.Delete(d);
            }
            else
            {
                DirectoryInfo d1 = new DirectoryInfo(d);
                if (d1.GetFiles().Length != 0)
                {
                    DeleteFolder(d1.FullName); //递归删除子文件夹
                }
                Directory.Delete(d);
            }
        }
    }

    public static long GetFileSize(string path)
    {
        System.IO.FileInfo info = new System.IO.FileInfo(path);
        return info.Length;
    }

    public static long GetDirectorySize(string path)
    {
        if (!Directory.Exists(path))
        {
            return 0;
        }

        long size = 0;

        DirectoryInfo info = new DirectoryInfo(path);
        foreach (FileInfo fi in info.GetFiles())
        {
            size += fi.Length;
        }

        foreach (DirectoryInfo di in info.GetDirectories())
        {
            size += GetDirectorySize(di.FullName);
        }
        return size;
    }

    /// <summary>
    /// 文件内容是否相同
    /// </summary>
    /// <param name="filePath1"></param>
    /// <param name="filePath2"></param>
    /// <returns></returns>
    public static bool CompareHash(string filePath1, string filePath2)
    {
        //创建一个哈希算法对象 
        using (HashAlgorithm hash = HashAlgorithm.Create())
        {
            using (FileStream file1 = new FileStream(filePath1, FileMode.Open), file2 = new FileStream(filePath2, FileMode.Open))
            {
                byte[] hashByte1 = hash.ComputeHash(file1);//哈希算法根据文本得到哈希码的字节数组 
                byte[] hashByte2 = hash.ComputeHash(file2);
                string str1 = BitConverter.ToString(hashByte1);//将字节数组装换为字符串 
                string str2 = BitConverter.ToString(hashByte2);
                Debug.LogFormat("{0}>>{1}>>{2}", str1, str2, str1 == str2);
                return str1 == str2;//比较哈希码
            }
        }
    }

    #region Create

    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(path);
    }

    public static void SaveTextToFile(string path, string fileName, string content)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        SaveBytesToFile(path, fileName, bytes);
    }

    public static void SaveBytesToFile(string path, string fileName, byte[] bytes)
    {
        CreateDirectory(path);
        try
        {
            var filePath = Path.Combine(path, fileName);
            Stream stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }
    #endregion

    #region Delete

    public static void DeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }

    public static void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }
    #endregion

    #region Read

    public static string ReadTextFromFile(string path, string defaultValue = "")
    {
        string ret = defaultValue;

        FileInfo fi = new FileInfo(path);
        if (fi.Exists)
        {
            StreamReader reader = fi.OpenText();
            ret = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
        }

        return ret;
    }
    #endregion

    #region Write
    //写入文件
    public static void WriteFile(string filePath, string text)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
        try
        {
            UTF8Encoding end = new UTF8Encoding(false);
            using (StreamWriter sw = new StreamWriter(filePath, false, end))
            {
                sw.Write(text);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    #endregion

    #region GetFile
    public static List<string> GetAllChildFiles(string path, string suffix = null, List<string> files = null)
    {
        if (files == null)
        {
            files = new List<string>();
        }

        AddFiles(path, suffix, files);

        string[] temps = Directory.GetDirectories(path);
        for (int i = 0; i < temps.Length; ++i)
        {
            string dir = temps[i];
            GetAllChildFiles(dir, suffix, files);
        }

        return files;
    }

    public static void AddFiles(string path, string suffix, List<string> files)
    {
        string[] temps = Directory.GetFiles(path);
        for (int i = 0; i < temps.Length; ++i)
        {
            string file = temps[i];
            if (string.IsNullOrEmpty(suffix) || file.ToLower().EndsWith(suffix.ToLower()))
            {
                files.Add(file);
            }
        }
    }
    #endregion

    #region Log
    private static string LogPath { get { return Application.persistentDataPath + "/Logs"; } }
    public static void WriteLog(string title, string content)
    {
        var date = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = date;
        if (!string.IsNullOrEmpty(title))
        {
            fileName = string.Format("{0}_[{1}]", date, title);
        }
        fileName = fileName + ".txt";
        SaveTextToFile(LogPath, fileName, content);
        Debug.Log("WriteLog:" + LogPath + "/" + fileName);
    }
    #endregion

    // --------------------------------------------------------------------------------------------
    private static string[] SearchWithRegex(string pathRoot, string[] paths, string pattern)
    {
        List<string> result = new List<string>();
        if (!pathRoot.EndsWith("/"))
        {
            pathRoot += "/";
        }

        Regex regex = new Regex(pattern.ToLower());
        for (int i = 0; i < paths.Length; ++i)
        {
            var path = paths[i];
            if (regex.Match(path.Replace(pathRoot, "").ToLower()).Success)
            {
                result.Add(path);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// 搜索文件夹
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="pattern">搜索规则</param>
    /// <param name="option">搜索选项</param>
    /// <returns></returns>
    public static string[] GetDirectories(string path, string pattern = "*.*", SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
        {
            return new string[] { };
        }

        if (string.IsNullOrEmpty(pattern))
        {
            pattern = "*.*";
        }
        else if (pattern.StartsWith("^") || pattern.EndsWith("$"))
        {
            // 如果是正则
            string[] paths = FileHelper.GetDirectories(path, "*.*", option);
            return SearchWithRegex(path, paths, pattern);
        }

        return Directory.GetDirectories(path, pattern, option);
    }

    /// <summary>
    /// 搜索文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="pattern">搜索规则</param>
    /// <param name="option">搜索选项</param>
    /// <returns></returns>
    public static string[] GetFiles(string path, string pattern = "*.*", SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
        {
            return new string[] { };
        }

        if (string.IsNullOrEmpty(pattern))
        {
            pattern = "*.*";
        }
        else if (pattern.StartsWith("^") || pattern.EndsWith("$"))
        {
            // 如果是正则
            string[] paths = FileHelper.GetFiles(path, "*.*", option);
            return SearchWithRegex(path, paths, pattern);
        }

        return Directory.GetFiles(path, pattern, option);
    }
}