using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Flatwhite
{
    /// <summary>
    /// A cache profile provider from a file using simple yaml format
    /// </summary>
    public class YamlCacheProfileProvider : IOutputCacheProfileProvider
    {
        private readonly IDictionary<string, IDictionary<string, string>> _yamlData;

        /// <summary>
        /// Initialize a YamlCacheProfileProvider with yamlProfileFilePath
        /// </summary>
        /// <param name="yamlProfileFile"></param>
        [ExcludeFromCodeCoverage]
        public YamlCacheProfileProvider(string yamlProfileFile)
        {
            var lines = File.Exists(yamlProfileFile) ? File.ReadAllLines(yamlProfileFile) : new string[0];
            _yamlData = ReadYamlData(lines.ToList());
        }

        internal static IDictionary<string, IDictionary<string, string>> ReadYamlData(List<string> lines)
        {
            var result = new Dictionary<string, IDictionary<string, string>>();
            var current = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.Trim(' ', '\t').StartsWith("-"))
                {
                    continue;
                }

                if (line.StartsWith("\t") || line.StartsWith("    "))
                {
                    var index = line.IndexOf(":", StringComparison.Ordinal);
                    if (index < 0)
                    {
                        throw new InvalidDataException($"Invalid yaml file at line {line}");
                    }
                    var key = line.Substring(0, index).Trim('\t', ' ');
                    var val = line.Substring(index + 1).Trim('\t', ' ');
                    current[key] = val;
                }
                else
                {
                    var profileName = line.Trim('\t', ' ');
                    current = new Dictionary<string, string>();
                    result[profileName] = current;
                }
            }
            return result;
        }

        /// <summary>
        /// Get a full profile base on profileName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public void ApplyProfileSetting<T>(T obj, string profileName) where T : class, new()
        {
            if (!_yamlData.ContainsKey(profileName))
            {
                return;
            }

            var data = _yamlData[profileName];
            var properties = typeof (T).GetProperties();
            foreach (var p in properties)
            {
                if (data.ContainsKey(p.Name) && data[p.Name] != null)
                {
                    try
                    {
                        p.SetValue(obj, Convert.ChangeType(data[p.Name], p.PropertyType));
                    }
                    catch (Exception ex)
                    {
                        Global.Logger.Error($"Cannot set [{data[p.Name]}] to property {p.Name} of type {typeof (T).Name}", ex);
                    }
                }
            }
        }
    }
}