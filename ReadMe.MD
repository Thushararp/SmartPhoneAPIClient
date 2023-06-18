.net Framework - .net6
Logging Library used: nLog by Microsoft

Solution architecture:
	- This solution uses the layered architecture approach to stucture the projects
	- In the service layer interfaces for the services used in the application are placed in separate projects from their implmentation. 
	  This enables any projects wanting to consume these services to just have a dependecy on their contract project, 
	  and do not have to inherit any dependancies from the actual service implmentation projects.
	- There is a custom logger service which currently utlise the external logger library nLog. Having the logger service separate will enable to switch out logger library down the line if need be. 
	
Presentation Layer / Composition Root:
	- Console App Project 'SmartPhoneAPIClient'
		This project contains the entry point of the application.
		The Program.cs uses the built in DI container to register the services used in the application	
			- LoggerManager
			- AuthenticationService
			- SmartPhoneService
		This project is both the Presentation layer as well as the composition root of the application
		Didn't separate this to two further projects to have a separate composition root project (console app) and a separate class library project for the presentation layer. 
		
Service / Business Logic Layer:		
	- Service.Contract
		This project contains the interfaces for the Authentication and SmartPhone services	
	- Service
		This project contains the actual implmentation of the service interfaces defined in the Service.Contract project.
		
	- LoggerService.Contract
		This project contains the interface for the LoggerMananger service.	
	- LoggerService
		This project contains the actual implmentation of the LoggerMananger interface defined in the oggerService.Contract project
		
Unit Tests:
- SmartPhoneAPIClientTest
	This project containts unit tests for the services used in this application.
	
	