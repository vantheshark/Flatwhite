using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Flatwhite
{
    /// <summary>
    /// Default cache key provider
    /// </summary>
    public class DefaultCacheKeyProvider : ICacheKeyProvider
    {
        private readonly ICacheAttributeProvider _cacheAttributeProvider;

        /// <summary>
        /// Initialize a default cache key provider using <see cref="ICacheAttributeProvider"/>
        /// </summary>
        /// <param name="cacheAttributeProvider"></param>
        public DefaultCacheKeyProvider(ICacheAttributeProvider cacheAttributeProvider)
        {
            _cacheAttributeProvider = cacheAttributeProvider;
        }

        private readonly IDictionary<MethodInfo, Tuple<string[], string[]>> _varyParamsCache = new Dictionary<MethodInfo, Tuple<string[], string[]>>();
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

            if (!_varyParamsCache.ContainsKey(invocation.Method))
            {
                var cacheAttribute = _cacheAttributeProvider.GetCacheAttribute(invocation.Method, invocationContext);
                var a = (cacheAttribute.VaryByParam ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var b = (cacheAttribute.VaryByCustom ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                _varyParamsCache[invocation.Method] = new Tuple<string[], string[]>(a, b);
            }

            var varyByParams = _varyParamsCache[invocation.Method].Item1;
            var varyByCustoms = _varyParamsCache[invocation.Method].Item2;

            var parameters = invocation.Method.GetParameters();
            if (parameters.Length > 0)
            {
                for (var i=0; i < parameters.Length; i++)
                {
                    var arg = invocation.GetArgumentValue(i);
                    var argKey = varyByParams.Contains(parameters[i].Name) ? (arg?.ToString() ?? "null") : "*";
                    key.Append($"{parameters[i].ParameterType.Name}:{argKey}, ");
                }
            }

            key.Remove(key.Length - 2, 2);
            key.Append(") :: ");
            foreach (var custom in varyByCustoms)
            {
                if (invocationContext.ContainsKey(custom))
                {
                    key.Append($" {custom}:{invocationContext[custom]?.ToString() ?? "null"}");
                }
            }
            return key.ToString();
        }
    }
}