using BackgroundLogService.Abstractions;
using BackgroundLogService.Extensions;
using BackgroundLogService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace BackgroundLogService.Infrastructure;

/// <summary>
/// Default implementation of ILogFilter for filtering sensitive data
/// </summary>
public class DefaultLogFilter : ILogFilter
{
    private readonly BackgroundLogServiceConfig _config;

    public DefaultLogFilter(IOptions<BackgroundLogServiceConfig> config)
    {
        _config = config.Value;
    }

    public bool ShouldIgnoreMethod(string? method, string sourceName)
    {
        if (string.IsNullOrEmpty(method)) return false;
        
        if (_config.FilterLogBySources.TryGetValue(sourceName, out var filterConfig))
        {
            return filterConfig.IgnoreByMethodList.Contains(method);
        }
        return false;
    }

    public object? ApplyFilters(object? data, string sourceName)
    {
        if (data == null) return null;

        if (!_config.FilterLogBySources.TryGetValue(sourceName, out var filterConfig))
        {
            return data.ToJson();
        }

        try
        {
            var json = data.ToJson();
            if (string.IsNullOrEmpty(json)) return data;

            var jToken = JToken.Parse(json);
            ApplyFiltersRecursive(jToken, filterConfig.FilterList);
            return jToken.ToString();
        }
        catch
        {
            return data.ToJson();
        }
    }

    private void ApplyFiltersRecursive(JToken token, List<FilterItem> filters)
    {
        if (token is JObject jObject)
        {
            foreach (var property in jObject.Properties().ToList())
            {
                var filter = filters.FirstOrDefault(f => f.PrototypeList.Contains(property.Name));
                if (filter != null && property.Value.Type == JTokenType.String)
                {
                    property.Value = ApplyFilter(property.Value.ToString(), filter);
                }
                else
                {
                    ApplyFiltersRecursive(property.Value, filters);
                }
            }
        }
        else if (token is JArray jArray)
        {
            foreach (var item in jArray)
            {
                ApplyFiltersRecursive(item, filters);
            }
        }
    }

    private static string ApplyFilter(string value, FilterItem filter)
    {
        return filter.Type switch
        {
            FilterType.Hidden => filter.ReplaceBy,
            FilterType.PartHidden => ApplyPartialHidden(value, filter),
            FilterType.Regex when !string.IsNullOrEmpty(filter.Pattern) => 
                System.Text.RegularExpressions.Regex.Replace(value, filter.Pattern, filter.ReplaceBy),
            _ => value
        };
    }

    private static string ApplyPartialHidden(string value, FilterItem filter)
    {
        if (string.IsNullOrEmpty(value)) return value;
        
        var start = Math.Min(filter.Start, value.Length);
        var end = filter.End > 0 ? Math.Min(filter.End, value.Length) : value.Length;
        var length = filter.Length > 0 ? filter.Length : (end - start);
        
        if (start + length > value.Length) length = value.Length - start;
        
        return value[..start] + new string('*', length) + value[(start + length)..];
    }
}


