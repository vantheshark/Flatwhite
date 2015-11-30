<img alt="Flatwhite logo" src="https://dl.dropboxusercontent.com/u/81698224/nuget-logos/coffee-.png" title="Flatwhite" width="100px" height="100px"/>

# Flatwhite (Nov 27 2015) [![Latest version](https://img.shields.io/nuget/v/Flatwhite.svg)](https://www.nuget.org/packages?q=flatwhite) [![Build Status](https://api.travis-ci.org/vanthoainguyen/Flatwhite.svg)](https://travis-ci.org/vanthoainguyen/Flatwhite) [![Build status](https://ci.appveyor.com/api/projects/status/rsognbyobn8fbasj?svg=true)](https://ci.appveyor.com/project/vanthoainguyen/flatwhite) [![Coverage Status](https://coveralls.io/repos/vanthoainguyen/Flatwhite/badge.svg?branch=master&service=github)](https://coveralls.io/github/vanthoainguyen/Flatwhite?branch=master) [![License WTFPL](https://img.shields.io/badge/licence-WTFPL-green.svg)](http://sam.zoy.org/wtfpl/COPYING)

Flatwhite is an AOP library with MVC and WebAPI ActionFilter style using Castle dynamic proxy. 
You can create MethodFilterAttribute to add custom logic to any methods as soon as it is interceptable by Castle Dynamic Proxy. 
Flatwhite has 1 built-in OutputCacheFilter to cache method result which can auto refresh stale content. 
You can use Flatwhite simply for caching or extending behavior of your methods such as profiling, logging by implement MethodFilterAttribute similar to MVC's ActionFilterAttribute

** Required packages:

[![Autofac](https://img.shields.io/badge/Autofac-3.5.2-yellow.svg)](https://www.nuget.org/packages/Autofac/3.5.2)

[![Castle.Core](https://img.shields.io/badge/Castle.Core-3.3.3-yellow.svg)](https://www.nuget.org/packages/Castle.Core/3.3.3)

## Usage: 

Your function needs to be **virtual** to enable the cache on class. Or the class needs to implement an interface and be registered as that interface type.

#### 1/ Enable class interceptor
If you modify the class to make the method virtual and decorate the method with **OutputCacheAttribute**, you will register the class like this:

```C#
public class UserService
{
    [OutputCache(Duration = 2, VaryByParam = "userId")]
    public virtual object GetById(Guid userId) 
	{
		// ...
	}    
}
var builder = new ContainerBuilder().EnableFlatwhite();
builder
	.RegisterType<CustomerService>()	
	.EnableInterceptors();
```

#### 2/ Enable interface interceptor
If the methods are not virtual, but the class implements an interface, you can decorate the methods on the interface with **OutputCacheAttribute** and register the type like this

```C#
public interface IUserService
{
    [OutputCache(Duration = 2, VaryByParam = "userId")]
    object GetById(Guid userId);

    [NoCache]
    object GetByEmail(string email);

    IEnumerable<object> GetRoles(Guid userId);
}

var builder = new ContainerBuilder().EnableFlatwhite();
builder.RegisterType<UserService>()	  
	   .As<IUserService>()	 
       .EnableInterceptors();
```

#### 3/ Quick enable cache on all methods
If you don't want to decorate the **OutputCache** attribute on the interface, you can do like this to enable cache on *all* methods

```C#
var builder = new ContainerBuilder().EnableFlatwhite();
builder.RegisterType<UserService>()	  
	   .As<IUserService>()	 
       .CacheWithStrategy(CacheStrategies
			.AllMethods()
			.Duration(5)
			.VaryByParam("userId")
	    );
```

#### 4/ Choose the method to cache without using Attribute filter
If you want to cache on just some of the methods, you can selectively do like below. Again, it works only on virtual methods if you are registering class service, interface service is fine.

```C#
var builder = new ContainerBuilder().EnableFlatwhite();
builder.RegisterType<BlogService>()
	   .As<IBlogService>()
	   .CacheWithStrategy(
	           CacheStrategies.ForService<IBlogService>()
						      .ForMember(x => x.GetById(Argument.Any<Guid>()))
							  .Duration(2)
							  .VaryByParam("postId")
                        
							  .ForMember(x => x.GetComments(Argument.Any<Guid>(), Argument.Any<int>()))
						      .Duration(2)
							  .VaryByCustom("custom")
							  .VaryByParam("postId")
							  .WithChangeMonitors((i, context) => 
							  {
									return new[] {new YouCustomCacheChangeMonitor()};
							  })									
       );
```

#### 5/ Enable interceptors on all previous registrations
If you're a fan of assembly scanning, you can decorate the *OutputCache* attribute on classes & interfaces you want to cache and enable them by registering **FlatwhiteBuilderInterceptModule** before building the container

```C#
var builder = new ContainerBuilder();
builder
		.RegisterType<BlogService>()
		.AsImplementedInterfaces()
		.AsSelf();

// Register other types normally
...

// Register FlatwhiteBuilderInterceptModule at the end
builder.RegisterModule<FlatwhiteBuilderInterceptModule>();            
var container = builder.Build();
```
Note that you don't have to call EnableFlatwhite() after creating ContainerBuilder like the other methods.

#### 6/ Auto refresh stale data
Flatwhite can auto refresh the stale content if you set **StaleWhileRevalidate** with a value greater than 0.
This should be used with Duration to indicates that caches MAY serve the cached result in which it appears after it becomes stale, up to the indicated number of seconds
The first call comes to the service and gets a stale cache result will also make the cache system auto refresh once. So if the method is not called many times in a short period, it's better to turn on AutoRefresh to make the cache refresh as soon as it starts to be stale
		

```C#
public interface IBlogService
{
	// For method with too many cache variations because of VaryByParam settings
    [OutputCache(Duration = 5, VaryByParam = "tag, from, authorId", StaleWhileRevalidate = 5)]
    IEnumerable<object> Search(string tag, DateTime from, Guid authorId);    
	
	
	// For method with not many cache variations and data is likely to changed every 5 seconds
    [OutputCache(Duration = 5, VaryByParam = "blogId", StaleWhileRevalidate = 5)]
    object GetById(int blogId);    
	
	
	// You can turn on AutoRefresh to keep the cache active if there are limited variations of the cache
    [OutputCache(Duration = 5, VaryByParam = "blogId", StaleWhileRevalidate = 5, AutoRefresh = true)]
    IEnumerable<string> GetBlogCategory();    
}
```

#### 7/ Revalidate cache
Even though you can use AutoRefresh or StaleWhileRevalidate to auto refresh cache data. Some time you want to remove the cache item after you call a certain method. You can use RevalidateAttribute to remove the cache item or some related cache items. Decorate the attribute on another method and the cache item will be removed once the method is invoked. On example below, when you call method DisableUser, because it has the Revalidate attribute decorated with "User" as the key, all related caches created for method with attribute OutputCache which has RevalidationKey = "User" will be reset.

```C#
public interface IUserService
{
    [OutputCache(Duration = 2, StaleWhileRevalidate = 2, VaryByParam = "userId", RevalidationKey = "User")]
	object GetById(Guid userId);

	[OutputCache(Duration = 2, VaryByParam = "userId", RevalidationKey = "User")]
	Task<object> GetByIdAsync(Guid userId);	

	[Revalidate("User")]
	void DisableUser(Guid userId);  
}
```

Unfortunately, this is not working for distributed services. That means the method is called on one server cannot notify the other service instances on remote servers. 
However, it's technically achievable to extend this filter using queueing or something like that to notify remote system.


#### 7/ Implement a filter
Flatwhite is inspired by WebAPI and ASP.NET MVC ActionFilterAttribute, so it works exactly the same. The base filter attribute has following methods. So simply implement your filter class and do whatever you want.

```C#
public abstract class MethodFilterAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the order in which the action filters are executed.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="methodExecutingContext"></param>
    public virtual void OnMethodExecuting(MethodExecutingContext methodExecutingContext)
    {
    }

    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="methodExecutingContext"></param>
    /// <returns></returns>
    public virtual Task OnMethodExecutingAsync(MethodExecutingContext methodExecutingContext)
    {
        try
        {
            OnMethodExecuting(methodExecutingContext);
        }
        catch (Exception ex)
        {
            TaskHelpers.FromError(ex);
        }

        return TaskHelpers.DefaultCompleted;
    }


    /// <summary>
    ///  Occurs after the action method is invoked.
    /// </summary>
    /// <param name="methodExecutedContext"></param>
    public virtual void OnMethodExecuted(MethodExecutedContext methodExecutedContext)
    {
    }

    /// <summary>
    ///  Occurs after the action method is invoked.
    /// </summary>
    /// <param name="methodExecutedContext"></param>
    /// <returns></returns>
    public virtual Task OnMethodExecutedAsync(MethodExecutedContext methodExecutedContext)
    {
        try
        {
            OnMethodExecuted(methodExecutedContext);
        }
        catch (Exception ex)
        {
            TaskHelpers.FromError(ex);
        }

        return TaskHelpers.DefaultCompleted;
    }
}
```

#### 7/ Extend or change the framework:
The Flatwhite.Global class has following static providers that are used in the code, you can change any of them. Usecases such as changing CacheStrategyProvider, ServiceActivator or Logger

```C#
/// <summary>
/// Context provider
/// </summary>
public static IContextProvider ContextProvider { get; set; }
/// <summary>
/// Cache key provider
/// </summary>
public static ICacheKeyProvider CacheKeyProvider { get; set; }
/// <summary>
/// Cache strategy provider
/// </summary>
public static ICacheStrategyProvider CacheStrategyProvider { get; set; }
        
/// <summary>
/// Attribute provider
/// </summary>
public static IAttributeProvider AttributeProvider { get; set; }
/// <summary>
/// Parameter serializer provider
/// </summary>
public static IHashCodeGeneratorProvider HashCodeGeneratorProvider { get; set; }

/// <summary>
/// A provider to resolve cache stores
/// </summary>
public static ICacheStoreProvider CacheStoreProvider { get; set; }

/// <summary>
/// The service activator to create instance of service when needed to invoke the MethodInfo for cache refreshing
/// </summary>
public static IServiceActivator ServiceActivator { get; set; }

/// <summary>
/// Logger
/// </summary>
public static ILogger Logger { get; set; }
```


## TODO:

Better documents

Support other IOC library


## WIKI:

https://github.com/vanthoainguyen/Flatwhite/wiki


## LICENCE
[![License WTFPL](https://img.shields.io/badge/licence-WTFPL-green.svg)](http://sam.zoy.org/wtfpl/COPYING) ![Troll](http://i40.tinypic.com/2m4vl2x.jpg) 