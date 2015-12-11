___________.__          __          .__    .__  __          
\_   _____/|  | _____ _/  |___  _  _|  |__ |__|/  |_  ____  
 |    __)  |  | \__  \\   __\ \/ \/ /  |  \|  \   __\/ __ \ 
 |     \   |  |__/ __ \|  |  \     /|   Y  \  ||  | \  ___/ 
 \___  /   |____(____  /__|   \/\_/ |___|  /__||__|  \___  >
     \/              \/                  \/              \/ 
	  
Flatwhite
=========



This demo project uses both Flatwhite.WebApi to cache the action methods and Flatwhite to cache the output of the interface method calls

You can see that the CoffeeController doesn't have OutputCache attribute decorated anywhere on the class but it uses a ICoffeeService intercepted by Flatwhite.OutputCache