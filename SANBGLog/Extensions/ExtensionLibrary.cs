using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace BackgroundLogService.Extensions;

public static class ExtensionLibrary
{
    #region JSON Utilities

    public static string ToJson(this object? obj)
    {
        if (obj == null) return "null";
        try
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            });
        }
        catch
        {
            return obj.ToString() ?? "null";
        }
    }

    public static string ToJsonIndented(this object? obj)
    {
        if (obj == null) return "null";
        try
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Include
            });
        }
        catch
        {
            return obj.ToString() ?? "null";
        }
    }

    public static T? FromJson<T>(this string? json)
    {
        if (string.IsNullOrEmpty(json)) return default;
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public static T? DeepClone<T>(this T? obj)
    {
        if (obj == null) return default;
        var json = obj.ToJson();
        return json.FromJson<T>();
    }

    public static JToken? TryParseJson(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JToken.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region DateTime Utilities

    public static string ToLogDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("yyyyMMdd");
    }

    public static string ToLogTimestampFormat(this DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }

    #endregion

    #region String Utilities

    public static string MaskValue(this string? value, int showFirst = 3, int showLast = 3, char maskChar = '*')
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var length = value.Length;
        if (length <= showFirst + showLast)
        {
            return new string(maskChar, length);
        }

        var masked = new string(maskChar, length - showFirst - showLast);
        return value.Substring(0, showFirst) + masked + value.Substring(length - showLast);
    }

    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Length <= maxLength) return value;
        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static bool IsValidJson(this string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        json = json.Trim();
        if ((json.StartsWith("{") && json.EndsWith("}")) ||
            (json.StartsWith("[") && json.EndsWith("]")))
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public static string ApplyRegexReplace(this string? value, string pattern, string replacement)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(pattern)) return value ?? string.Empty;
        try
        {
            return Regex.Replace(value, pattern, replacement);
        }
        catch
        {
            return value;
        }
    }

    #endregion
}
