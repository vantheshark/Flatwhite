using System;

namespace Flatwhite
{
    /// <summary>
    /// Use this attribute to decorate on a method where we don't want to cache while the class or interface decorated with OutputCache attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NoCacheAttribute : Attribute
    {
    }
}