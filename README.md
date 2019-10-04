# Steeltoe Demonstration

This repository contains code that demonstrates the use of [Steeltoe](https://steeltoe.io/) to implement .NET microservices. The Steeltoe features Config Server, Service Discovery, and Circuit Breaker are all demonstrated.

Note that the demonstration of Config Server retrieves its configuration from the https://github.com/fjb4/steeltoe-config-repo repository.

TODO
- Rename services?
- Break into mutiple git repos?
  - Repo for the service itself
  - Repo for service creation
    - docker-compose.yml
    - config-server-json
    - Create readme containing service creation parts of readme below

## How to Run with Docker (local)
- Prerequisites
  - [.NET Core SDK](https://dotnet.microsoft.com/download) (tested with version 2.2)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop) (tested with 2.1)
- Start Steeltoe services
  - `docker-compose up -d`
- Stop Steeltoe services
  - `docker-compose down`
- Run
  - Open a terminal window
    - Set the "BUILD" environment variable to have the value "LOCAL"
      - Unix Bash: `export BUILD=LOCAL`
      - Windows CMD: `set BUILD=LOCAL`
    - `dotnet watch run --launch-profile=backend`
  - Open a second terminal window
    - Set the "BUILD" environment variable to have the value "LOCAL"
    - `dotnet watch run --launch-profile=frontend`
- URLs
  - Frontend Service: `http://localhost:5000`
  - Backend Service: `http://localhost:5100`
  - Config Server: `http://localhost:8888`
  - Service Registry: `http://localhost:8761`
  - Circuit Breaker Dashboard: `http://localhost:7979`


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
  - This is a very simple ASP.NET web project (created with `dotnet new web`)
    - Not MVC, not API
    - All incoming requests are handled in Startup.Configure()
    - Intent is to distill code down to minimum necessary for demonstrating Steeltoe
- Demonstrate backend service in browser
    - Draws a block containing information about the service
    - Concatenates any information received from its upstream host
      - For backend, upstream host is date.jsontest.com
      - Public service that returns date/time as JSON
- Give high-level overview of service implementation
    - Only 3 C# files
        - Program.cs
        - Startup.cs
        - GetUpstreamContentCommand.cs
    - Requests are handled in Start.Configure()
    - Retrieves color & upstream host from configuration
        - Should look familiar, code is using the normal .NET configuration API
    - Renders HTML that displays service's name & color
    - Calls upstream host and concatenates its response
        - Acknowledge that the code used to retrieve the upstream host's content is in a separate GetUpstreamContentCommand class; the reasons for this will be discussed later 
- [Config Server](https://steeltoe.io/docs/steeltoe-configuration/#2-0-config-server-provider) Demonstration
  - Utilizes Spring Cloud Config Server
    - Supports different backends: file system, git repo, database, others
    - Config Server itself must be configured to tell it where to pull configuration data
      - Show config-server.json file
  - Show the [configuration repo](https://github.com/fjb4/steeltoe-config-repo)
    - Application's name determines which YAML file is used
  - Show code changes necessary to support config server
    - Steeltoe.Extensions.Configuration.ConfigServerCore NuGet package
    - AddConfigServer() in Program.cs
    - appsettings.json can be used to point to config server location
    - Retrieving configuration values is standard .NET syntax
- Demonstrate other service instance
  - Show backend, then frontend in browser
  - Design allows services to be chained together
      - Frontend -> backend -> date.jsontest.com
      - Multiple instances of the same service with different configurations
- [Service Discovery](https://steeltoe.io/docs/steeltoe-discovery/) Demonstration
  - How are the different service instances able to communicate?
    - Clients don't know fully qualified URL of upstream host
  - Show service registry with list of registered apps
    - This is the "phone book" that allows apps to lookup a service's URL
  - Show code changes necessary to implement service discovery
    - Steeltoe.Discovery.ClientCore NuGet package
    - Show AddDiscoveryClient() in Startup.cs
    - HttpClient has been augmented with DiscoveryHttpClientHandler in GetUpstreamContentCommand class
  - Run multiple instances of the backend service
    - `cf scale backend -i 3`
  - Wait for additional service instances to appear in service registry
  - Show how calls to backend service are load balanced across the multiple service instances
    - In the browser, each service has a number in parentheses after its name
      - This is the application instance index
    - Refresh the frontend service several times and the backend's instance index should change
- [Circuit Breaker](https://steeltoe.io/docs/steeltoe-circuitbreaker/) Demonstration
  - Show the Circuit Breaker Dashboard
    - Note that all the circuits are closed
  - Show code changes needed to implement circuit breaker
    - Steeltoe.CircuitBreaker.HystrixCore NuGet package
    - Show AddHystrixCommand() & AddHystrixMetricsStream() in Startup.cs
    - GetUpstreamContentCommand class that derives from HystrixCommand
      - Discuss RunAsync() vs RunFallbackAsync()
  - Demonstrate a circuit opening
    - Kill one of the services: `cf stop backend`
    - Refresh the frontend service a few times
      - Backend becomes red, backend service is no longer being called
      - Dashboard shows that circuit remains closed
      - While circuit is closed, each call to backend still attempts to execute RunAsync() but, when RunAsync() fails, RunFallbackAsync() is invoked
        - Consumers of the service see the response from RunFallbackAsync() instead of an error
    - Now refresh the frontend service repeatedly until backend circuit flips open
      - Dashboard shows that circuit is open
      - While circuit is open, almost all calls to backend are "short circuited" and only RunFallbackAsync() is invoked
        - After some time, the circuit will enter a "half-open" state
        - In this state, a single request is allowed to pass through and, if the request succeeds, the circuit will close
        - Config
          - requestVolumeThreshold: Minimum number of requests in a rolling window that will trip the circuit (20)
          - sleepWindowInMilliseconds: Amount of time, after tripping the circuit, to reject requests before allowing attempts again (5000)
          - errorThresholdPercentage: Error percentage at or above which the circuit should trip open and start short-circuiting requests to fallback logic (50)
    - Restart the service that was killed: `cf start backend`
    - Refresh the frontend service until backend circuit closes again
    - Dashboard shows the circuit is now open again
    - Service is restored, each call to backend again calls RunAsync()
