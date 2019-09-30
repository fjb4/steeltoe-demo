# Steeltoe Demonstration

This repository contains code that demonstrates the use of [Steeltoe](https://steeltoe.io/) to implement .NET microservices. The Steeltoe features Config Server, Service Discovery, and Circuit Breaker are all demonstrated.

Note that the demonstration of Config Server retrieves its configuration from the https://github.com/fjb4/steeltoe-config-repo repository.

Some of the commands below are Unix-specific, but this should all run on Windows if you translate them to their Windows equivalents.

## How to Run on Docker (local)
- Prerequisites
  - [.NET Core SDK](https://dotnet.microsoft.com/download) (tested with version 2.2)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop) (tested with 2.1)
- Start Steeltoe services
  - `docker-compose up -d`
- Stop Steeltoe services
  - `docker-compose down`
- Run
  - `export BUILD=LOCAL`
    - If using multiple terminal windows, this needs to be set in each one
    - Make sure this is unset to run on Cloud Foundry
      - `unset BUILD`
  - `dotnet watch run --launch-profile=backend`
  - `dotnet watch run --launch-profile=middleware`
  - `dotnet watch run --launch-profile=frontend`


## How to Run on Cloud Foundry
- Prerequisites
  - [Create a Pivotal Web Services account](https://run.pivotal.io/)
  - [Install the Cloud Foundry CLI](https://pivotal.io/platform/pcf-tutorials/getting-started-with-pivotal-cloud-foundry/install-the-cf-cli)
  - Login to your Cloud Foundry account
    - `cf login -a https://api.run.pivotal.io`
- Create Steeltoe services
  - `cf create-service p-config-server trial myConfigServer -c ./config-server.json`
  - `cf create-service p-service-registry trial myDiscoveryService`
  - `cf create-service p-circuit-breaker-dashboard trial myHystrixService`
- Deploy and run
  - `cf push`


## Demo Script (Cloud Foundry)
- Demonstration of microservices
    - Very simple ASP.NET web project
        - Not MVC, not API
        - All incoming requests are handled in Startup.Configure()
    -  Intent is to demonstrate Steeltoe, removing any unnecessary code
- Demonstrate backend service
    - Draws a block containing information about the service
    - Concatenates any information received from the upstream host
- Show service implementation
    - Retrieves color & upstream host from configuration
    - Renders service info
    - Calls upstream host and concatenates response
    - Demonstrate backend, middleware, then frontend
    - Design allows services to be chained together
        - Frontend -> middleware -> backend -> date.jsontest.com
        - Multiple instances of the same service with different configuration
- Introduce [Apps Manager](https://run.pivotal.io/), show running services
- Config Server
  - Show configuration repo on GitHub
    - https://github.com/fjb4/steeltoe-config-repo
    - Application name chooses which YAML file is used
  - Show how configuration server points to GitHub repo
  - Show code that retrieves configuration values
    - appsettings.json can point to config server location
    - Retrieving configuration values is standard .NET syntax
  - Demonstrate changing the configuration?
- Service Discovery
  - Show service registry with list of registered apps
  - Show AddDiscoveryClient() in Startup.cs
  - Explain how it resolves the service URL in GetUpstreamContentCommand.RunAsync()
  - Run multiple instances of the middleware service
    - `cf scale middleware -i 3`
  - Wait for additional service instances to appear in service registry
  - Show how calls to middleware service are load balanced across the multiple service instances
- Circuit Breaker
  - Show code changes needed to implement circuit breaker
    - Show AddHystrixCommand() in Startup.cs
    - GetUpstreamContentCommand class that derives from HystrixCommand
      - RunAsync() vs RunFallbackAsync()
  - Show the Circuit Breaker Dashboard
  - Demonstrate a circuit opening
    - `cf stop middleware`
    - Refresh the frontend service a few times
      - Middleware displays "Service temporarily unavailable"
      - Dashboard shows that circuit remains closed
      - While circuit is closed, each call to middleware still attempts to execute RunAsync() but, when RunAsync() fails, RunFallbackAsync() is invoked
    - Refresh the frontend service until middleware circuit flips open
      - Dashboard shows that circuit is open
      - While circuit is open, each call to middleware is "short circuited" and only RunFallbackAsync() is invoked
    - `cf start middleware`
    - Refresh the frontend service until middleware circuit closes again
    - Dashboard shows the circuit is now open again
    - Service is restored, each call to middleware again calls RunAsync()
