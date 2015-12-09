___________.__          __          .__    .__  __          
\_   _____/|  | _____ _/  |___  _  _|  |__ |__|/  |_  ____  
 |    __)  |  | \__  \\   __\ \/ \/ /  |  \|  \   __\/ __ \ 
 |     \   |  |__/ __ \|  |  \     /|   Y  \  ||  | \  ___/ 
 \___  /   |____(____  /__|   \/\_/ |___|  /__||__|  \___  >
     \/              \/                  \/              \/ 
	  
Flatwhite
=========

GitHub https://github.com/vanthoainguyen/Flatwhite

 _    _ _           _     _       _ _   
| |  | | |         | |   (_)     (_) |  
| |  | | |__   __ _| |_   _ ___   _| |_ 
| |/\| | '_ \ / _` | __| | / __| | | __|
\  /\  / | | | (_| | |_  | \__ \ | | |_ 
 \/  \/|_| |_|\__,_|\__| |_|___/ |_|\__|
                                        
                                        

Flatwhite.WebApi is an high performance output cache library for WebApi with VaryByParam (on action method) and VaryByHeader support, facilitate usages of cache control and HTTP Cache-Control Extensions for Stale Content. It auto refreshes the stale content so the action method call will never wait. It intercepts the request at the earliest stage to see if cache data is available and return 304 if applicable. So most of the case, the WebAPI engine doesn't need to create API controllers and other stuff which boost the performance to the limit.

______           _                                   
| ___ \         (_)                                  
| |_/ / __ _ ___ _  ___   _   _ ___  __ _  __ _  ___ 
| ___ \/ _` / __| |/ __| | | | / __|/ _` |/ _` |/ _ \
| |_/ / (_| \__ \ | (__  | |_| \__ \ (_| | (_| |  __/
\____/ \__,_|___/_|\___|  \__,_|___/\__,_|\__, |\___|
                                           __/ |     
                                          |___/      

I/ Using with WebApi2

	GlobalConfiguration.Configure(WebApiConfig.Register);
	//NOTE: This is what you need for WebApi2
	GlobalConfiguration.Configure(x => x.UseFlatwhiteCache());

II/ Using with Owin

In your Startup.cs class:

	public void Configuration(IAppBuilder app)
	{
		var config = new HttpConfiguration();   
		WebApiConfig.Register(config);
		app.UseWebApi(config)
		   .UseFlatwhiteCache(config);
	}

III/ Using with Owin & Autofac

In your Startup.cs class:

	public void Configuration(IAppBuilder app)
	{
		var config = new HttpConfiguration();
		var container = BuildAutofacContainer(config);

		WebApiConfig.Register(config);

		app.UseWebApi(config)
		   .UseFlatwhiteCache(config);
	}

	private IContainer BuildAutofacContainer(HttpConfiguration config)
	{
		var builder = new ContainerBuilder().EnableFlatwhite();

		// This will also be set to Global.CacheStrategyProvider in UseFlatwhiteCache method
		builder.RegisterType<WebApiCacheStrategyProvider>().As<ICacheStrategyProvider>().SingleInstance();

		// This is required by EtagHeaderHandler and OutputCacheAttribute when it builds the response
		builder.RegisterType<CacheResponseBuilder>().As<ICacheResponseBuilder>().SingleInstance();

		// This is required by CachControlHeaderHandlerProvider
		// NOTE: Register more instances of ICachControlHeaderHandler here
		builder.RegisterType<EtagHeaderHandler>().As<ICachControlHeaderHandler>().SingleInstance();


		builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
		// OPTIONAL: Register the Autofac filter provider.
		builder.RegisterWebApiFilterProvider(config);


		var container = builder.Build();
		config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		return container;
	}

IV/ Decorate OutputCache attribute on your action method

	[OutputCache(
		MaxAge = 5,
		StaleWhileRevalidate = 5,
		VaryByParam = "*",		
		IgnoreRevalidationRequest = true)]
	public async Task<HttpResponseMessage> ActionMethod(int id)
	{
		// ...
	}
	
 _   _               _   _          _      ___  
| \ | |             | | | |        | |    |__ \ 
|  \| | ___  ___  __| | | |__   ___| |_ __   ) |
| . ` |/ _ \/ _ \/ _` | | '_ \ / _ \ | '_ \ / / 
| |\  |  __/  __/ (_| | | | | |  __/ | |_) |_|  
\_| \_/\___|\___|\__,_| |_| |_|\___|_| .__/(_)  
                                     | |        
                                     |_|        

Documentation can be found at github wiki page: https://github.com/vanthoainguyen/Flatwhite/wiki

 _     _____ _____  _____ _   _ _____  _____ 
| |   |_   _/  __ \|  ___| \ | /  __ \|  ___|
| |     | | | /  \/| |__ |  \| | /  \/| |__  
| |     | | | |    |  __|| . ` | |    |  __| 
| |_____| |_| \__/\| |___| |\  | \__/\| |___ 
\_____/\___/ \____/\____/\_| \_/\____/\____/                                            
                                            
http://sam.zoy.org/wtfpl/COPYING 




                     .@_¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦_@,                
                   .¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦.              
                 .,¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦y.            
              ,@_¦¦¦¦¦¦¦¦¦````````````````````````````````````+¦¦¦¦¦¦¦¦¦_@          
            .@¦¦¦¦¦¦¦¦¦¦¦+                                     `¦¦¦¦¦¦¦¦¦¦¦¦,       
           ;¦¦¦¦¦¦¦¦¦¦¯¯+                                       +¯¯¦¦¦¦¦¦¦¦¦¦,      
           ¦¦¦¦¦¦¦Ñ`                                                 `¦¦¦¦¦¦¦¦      
          ¦¦¦¦¦¦¦¦                                                     ¦¦¦¦¦¦¦w     
          ¦¦¦¦¦¦¦¦______________________________________________________¦¦¦¦¦¦¦     
          ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦     
          ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦     
           `+-¯¯¦¦¦¦¦¦¦¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¯¦¦¦¦¦¦¦¯¯++`      
                ¦¦¦¦¦¦¦                                          .¦¦¦¦¦¦¦           
                ¦¦¦¦¦¦¦¦                                         ¦¦¦¦¦¦¦¦           
                ]¦¦¦¦¦¦¦                                         ¦¦¦¦¦¦¦¦           
                ¦¦¦¦¦¦¦¦_________________________________________¦¦¦¦¦¦¦,           
               ÿ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦w          
               ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦          
               `¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¯¯Ñ²²Ñ¯¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦           
                ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¯+`       /¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦           
                ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¯+        ÿ¦¦¦ +¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦           
                +¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦        +_¦¦¦   ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦           
                 ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦        w¦¦¦¦Ñ    ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦            
                 ]¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦       ¡_¦¦¦¦`     ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦            
                 !¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦U     =_¦¦¦¯+     ,¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦            
                  ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦   q¦¦¦¦Ñ`     ,@¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦`            
                  +¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦.¦¦¦¯Ñ`     .µ_¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦             
                   ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦`   .,w@_¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦+             
                   ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦___¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦`             
                   ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦              
                    Ñ¯¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦`              
                      ¦¦¦¦¦¦¦¦`````````````````````````````¦¦¦¦¦¦¦¦                 
                      ]¦¦¦¦¦¦¦.                            ¦¦¦¦¦¦¦¦                 
                      .¦¦¦¦¦¦¦@µµµWµµµµWµµµµµµµµµµµµWµµµµW@¦¦¦¦¦¦¦                  
                       ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦                  
                        ¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦¦+                  
    
---
^[ [^ascii ^art ^generator](http://asciiart.club) ^] 


