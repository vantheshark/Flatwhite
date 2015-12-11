using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Flatwhite.Provider
{
    /// <summary>
    /// Default cache key provider
    /// </summary>
    public class DefaultCacheKeyProvider : ICacheKeyProvider
    {
        /// <summary>
        /// Hashcode generator provider
        /// </summary>
        protected readonly IHashCodeGeneratorProvider _hashCodeGeneratorProvider;

        /// <summary>
        /// Initialize a default cache key provider using <see cref="IHashCodeGeneratorProvider"/>
        /// </summary>
        /// <param name="hashCodeGeneratorProvider"></param>
        public DefaultCacheKeyProvider(IHashCodeGeneratorProvider hashCodeGeneratorProvider = null)
        {
            _hashCodeGeneratorProvider = hashCodeGeneratorProvider ?? Global.HashCodeGeneratorProvider;
        }

        /// <summary>
        /// Resolve cache key
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual string GetCacheKey(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            var info = invocationContext.TryGetByKey<ICacheSettings>(Global.__flatwhite_outputcache_attribute);
            if (info == null)
            {
                throw new InvalidOperationException($"{nameof(ICacheSettings)} object not found in {nameof(invocationContext)}");
            }

            // The cache key must be different for different instance of same type
            var key = new StringBuilder($"Flatwhite::{(invocation.Method.DeclaringType ?? invocation.TargetType).FullName}.{invocation.Method.Name}(");
            
            var varyByParams = (info.VaryByParam ?? "").Split(new [] {',',' '}, StringSplitOptions.RemoveEmptyEntries);
            var varyByCustoms = info.GetAllVaryCustomKey().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var parameters = invocation.Method.GetParameters();
            if (parameters.Length > 0)
            {
                BuildWithParams(invocation, parameters, varyByParams, key);
                key.Remove(key.Length - 2, 2);
            }
            
            key.Append(") :: ");

            if (varyByCustoms.Length > 0)
            {
                foreach (var custom in varyByCustoms)
                {
                    BuildWithCustom("", invocationContext, custom, key);
                    key.Append(", ");
                }
            }
            return key.ToString().TrimEnd(' ', ':', ',');
        }

        /// <summary>
        /// Build the key with provided varyByParams
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="invocationContext"></param>
        /// <param name="customKey"></param>
        /// <param name="key"></param>
        protected virtual void BuildWithCustom(string prefixKey, IDictionary<string, object> invocationContext, string customKey, StringBuilder key)
        {
            if (invocationContext.ContainsKey(customKey))
            {
                var customValue = invocationContext[customKey];
                var code = customValue == null
                    ? "null"
                    : _hashCodeGeneratorProvider.GetForType(customValue.GetType()).GetCode(customValue);
                key.Append($" {prefixKey}:{code}");
            }
            else
            {
                var indexOfDot = customKey.IndexOf(".", StringComparison.Ordinal);
                if (indexOfDot > 0)
                {
                    var prefix = customKey.Substring(0, indexOfDot);
                    var fieldName = customKey.Substring(indexOfDot + 1);
                    if (invocationContext.ContainsKey(prefix))
                    {
                        var value = invocationContext[prefix];
                        if (value is IDictionary<string, object>)
                        {
                            BuildWithCustom($"{prefixKey}{prefix}.{fieldName}", (IDictionary<string, object>)value, fieldName, key);
                        }
                        else
                        {
                            indexOfDot = fieldName.IndexOf(".", StringComparison.Ordinal);

                            try
                            {
                                if (indexOfDot < 0) // this should be a propertyName
                                {
                                    var pInfo = value.GetType().GetProperty(fieldName);
                                    var customValue = pInfo.GetValue(value, null);
                                    BuildWithCustom($"{prefixKey}{prefix}.{fieldName}", new Dictionary<string, object> {{fieldName, customValue}}, fieldName, key);
                                }
                                else // Still property chain
                                {
                                    var prefix2 = fieldName.Substring(0, indexOfDot);
                                    var pInfo = value.GetType().GetProperty(prefix2);
                                    var customValue = pInfo.GetValue(value, null);
                                    BuildWithCustom($"{prefixKey}{prefix}.", new Dictionary<string, object> { { prefix2, customValue } }, fieldName, key);
                                }
                            }
                            catch
                            {
                                // Ignore, probably the original customField has more than 1 dot which is not supported or the field after dot not found
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Build the key with provided varyByCustoms
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="parameters"></param>
        /// <param name="varyByParams"></param>
        /// <param name="key"></param>
        protected virtual void BuildWithParams(_IInvocation invocation, ParameterInfo[] parameters, string[] varyByParams, StringBuilder key)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var arg = invocation.GetArgumentValue(i);
                var argKey = "*";
                if (varyByParams.Contains("*") || varyByParams.Contains(parameters[i].Name))
                {
                    argKey = _hashCodeGeneratorProvider.GetForType(parameters[i].ParameterType).GetCode(arg);
                }
                key.Append($"{parameters[i].ParameterType.Name}:{argKey}, ");
            }
        }
    }
}