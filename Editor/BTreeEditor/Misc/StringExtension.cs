using System;
using System.Text;

public static class StringExtension
{
    #region string
    public static string ReplaceEmpty(this string source, string oldVlue)
    {
        return source.Replace(oldVlue, string.Empty);
    }

    public static string StandardPathLower(this string source)
    {
        return source.Replace("\\", "/").ToLower();
    }
    public static string StandardPath(this string source)
    {
        return source.Replace("\\", "/");
    }

    public static string ReplacePath(this string source, string oldValue, string newValue)
    {
        return source.StandardPathLower().Replace(oldValue.StandardPathLower(), newValue);
    }

    public static string SubstringTo(this string source, string value)
    {
        int index = source.IndexOf(value, StringComparison.Ordinal);
        if (index == -1) return source;
        return source.Substring(0, index);
    }

    public static string SubstringFrom(this string source, string value)
    {
        int index = source.IndexOf(value, StringComparison.Ordinal);
        if (index == -1) return source;
        return source.Substring(index + value.Length, source.Length - value.Length);
    }

    public static string SubstringStartsWith(this string source, string value)
    {
        if (source.StartsWith(value))
        {
            return source.SubstringFrom(value);
        }
        return source;
    }

    public static string GetFileNameLower(this string source)
    {
        string[] array = source.StandardPathLower().Split('/');
        return array[array.Length - 1];
    }
    
    public static string GetFileName(this string source)
    {
        string[] array = source.StandardPath().Split('/');
        return array[array.Length - 1];
    }
    public static string RemoveSuffix(this string source)
    {
        int index = source.LastIndexOf('.');
        if (index != -1)
        {
            return source.Remove(index);
        }
        return source;
    }
    #endregion
    public static void Clear(this StringBuilder sb)
    {
        sb.Length = 0;
    }
    public static void AppendLineEx(this StringBuilder sb, string str = "")
    {
        sb.Append(str + "\r\n");
    }

}
