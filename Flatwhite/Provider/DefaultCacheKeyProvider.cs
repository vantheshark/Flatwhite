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
        /// Cache attribute provider
        /// </summary>
        protected readonly ICacheAttributeProvider _cacheAttributeProvider;
        /// <summary>
        /// Hashcode generator provider
        /// </summary>
        protected readonly IHashCodeGeneratorProvider _hashCodeGeneratorProvider;

        /// <summary>
        /// Initialize a default cache key provider using <see cref="ICacheAttributeProvider"/>
        /// </summary>
        /// <param name="cacheAttributeProvider"></param>
        /// <param name="hashCodeGeneratorProvider"></param>
        public DefaultCacheKeyProvider(ICacheAttributeProvider cacheAttributeProvider, IHashCodeGeneratorProvider hashCodeGeneratorProvider)
        {
            if (cacheAttributeProvider == null)
            {
                throw new ArgumentNullException(nameof(cacheAttributeProvider));
            }
            if (hashCodeGeneratorProvider == null)
            {
                throw new ArgumentNullException(nameof(hashCodeGeneratorProvider));
            }
          

            _cacheAttributeProvider = cacheAttributeProvider;
            _hashCodeGeneratorProvider = hashCodeGeneratorProvider;
        }

        /// <summary>
        /// Resolve cache key
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="invocationContext"></param>
        /// <returns></returns>
        public virtual string GetCacheKey(_IInvocation invocation, IDictionary<string, object> invocationContext)
        {
            // The cache key must be different for different instance of same type
            var key = new StringBuilder($"Flatwhite::{(invocation.Method.DeclaringType ?? invocation.TargetType).FullName}.{invocation.Method.Name}(");

            if (!Global.Cache.VaryParamsCache.ContainsKey(invocation.Method))
            {
                var cacheAttribute = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
                var a = (cacheAttribute.VaryByParam ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var b = (cacheAttribute.VaryByCustom ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Global.Cache.VaryParamsCache[invocation.Method] = new Tuple<string[], string[]>(a, b);
            }
            
            var varyByParams = Global.Cache.VaryParamsCache[invocation.Method].Item1;
            var varyByCustoms = Global.Cache.VaryParamsCache[invocation.Method].Item2;

            var parameters = invocation.Method.GetParameters();
            if (parameters.Length > 0)
            {
                BuildWithParams(invocation, parameters, varyByParams, key);
            }

            key.Remove(key.Length - 2, 2);
            key.Append(") :: ");
            foreach (var custom in varyByCustoms)
            {
                BuildWithCustom(invocationContext, custom, key);
            }
            return key.ToString();
        }

        /// <summary>
        /// Build the key with provided varyByParams
        /// </summary>
        /// <param name="invocationContext"></param>
        /// <param name="customKey"></param>
        /// <param name="key"></param>
        protected virtual void BuildWithCustom(IDictionary<string, object> invocationContext, string customKey, StringBuilder key)
        {
            if (invocationContext.ContainsKey(customKey))
            {
                var customValue = invocationContext[customKey];
                var code = customValue == null
                    ? "null"
                    : _hashCodeGeneratorProvider.GetForType(customValue.GetType()).GetCode(customValue);
                key.Append($" {customKey}:{code}");
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
                            BuildWithCustom((IDictionary<string, object>)value, fieldName, key);
                        }
                        else
                        {
                            try
                            {
                                var pInfo = value.GetType().GetProperty(fieldName);
                                var customValue = pInfo.GetValue(value, null);
                                BuildWithCustom(new Dictionary<string, object> { { fieldName, customValue } }, fieldName, key);
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