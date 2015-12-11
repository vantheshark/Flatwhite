___________.__          __          .__    .__  __          
\_   _____/|  | _____ _/  |___  _  _|  |__ |__|/  |_  ____  
 |    __)  |  | \__  \\   __\ \/ \/ /  |  \|  \   __\/ __ \ 
 |     \   |  |__/ __ \|  |  \     /|   Y  \  ||  | \  ___/ 
 \___  /   |____(____  /__|   \/\_/ |___|  /__||__|  \___  >
     \/              \/                  \/              \/ 
	  
Flatwhite
=========


This demo project only uses Flatwhite.WebApi.OutputCache to cache the action methods.

The ICoffeeService doesn't have any Flatwhite.OutputCache enable with Autofac so the methods calls are not cached. Infact, you don't see Autofac dependency on this project.