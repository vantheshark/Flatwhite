# Flatwhite (1.0.7) (Nov 27 2015) 
** Method filter & output cache using Dynamic proxy with MVC and WebAPI action filter attribute style

** Required packages:

Autofac

Castle.Core

Flatwhite

Flatwhite.Autofac

Nuget: *https://www.nuget.org/packages/Flatwhite

## Usage: 

Your function needs to be **virtual** to enable the cache on class. Or the class needs to implement an interface and be registered as that interface type.

1/ If you can modify the class to make the method virtual and decorate the method with **OutputCacheAttribute**, you can register the class like this:

```C#
var builder = new ContainerBuilder().EnableFlatwhiteCache();
builder.RegisterType<CustomerService>()	   
       .CacheWithAttribute();
```

2/ If the methods are not virtual, but the class implements an interface, you can decorate the methods on the interface with **OutputCacheAttribute** and register the type like this

```C#
public interface IUserService
{
    [OutputCache(Duration = 2, VaryByParam = "userId")]
    object GetById(Guid userId);

    [NoCache]
    object GetByEmail(string email);

    IEnumerable<object> GetRoles(Guid userId);
}

var builder = new ContainerBuilder().EnableFlatwhiteCache();
builder.RegisterType<UserService>()	  
	   .As<IUserService>()	 
       .CacheWithAttribute();
```

3/ If you don't want to decorate the **OutputCache** attribute on the interface, you can do like this to enable cache on *all* methods

```C#
var builder = new ContainerBuilder().EnableFlatwhiteCache();
builder.RegisterType<UserService>()	  
	   .As<IUserService>()	 
       .CacheWithStrategy(CacheStrategies
			.AllMethods()
			.Duration(5)
			.VaryByParam("userId")
	    );
```

4/ If you want to cache on just some of the methods, you can selectively do like below. Again, it works only on virtual methods if you are registering class service, interface service is fine.

```C#
var builder = new ContainerBuilder().EnableFlatwhiteCache();
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
							  })
									return new[] {new YouCustomCacheChangeMonitor()};
       );
```

5/ If you're a fan of assembly scanning, you can decorate the *OutputCache* attribute on classes & interfaces you want to cache and enable them by registering **FlatwhiteBuilderInterceptModule** before building the container

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
Note that you don't have to call EnableFlatwhiteCache() on the builder like the other methods.

6/ Flatwhite can auto refresh the stale content if you set **StaleWhileRevalidate** with a value greater than 0

```C#
public interface IUserService
{
    [OutputCache(Duration = 2, StaleWhileRevalidate = 2)]
    object GetById(Guid userId);    
}
```

7/ You can use RevalidateAttribute to remove the cache item. Decorate it on another method and the cache item will be removed once the method is invoked.

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

This is not working for distributed services. That means the method is called on a server cannot notify the other service instance on remote server. However, it's technically doable to extend this filter.

## TODO:

Better documents

Support other IOC library


## WIKI:

https://github.com/vanthoainguyen/Flatwhite/wiki


## LICENCE
http://sam.zoy.org/wtfpl/COPYING 
![Troll](http://i40.tinypic.com/2m4vl2x.jpg) 