using System;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method where we don't want to cache while the class or interface decorated with OutputCache attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NoCacheAttribute : NoInterceptAttribute
    {
    }


    /// <summary>
    /// Use this attribute to decorate on a method where we don't want to intercept by Caslte dynamic proxy
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface)]
    public class NoInterceptAttribute : Attribute
    {
    }
}