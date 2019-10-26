# Steeltoe Demonstration

This repository contains code that demonstrates the use of [Steeltoe](https://steeltoe.io/) to implement .NET microservices. The Steeltoe features Config Server, Service Discovery, and Circuit Breaker are all demonstrated.

Note that the demonstration of Config Server retrieves its configuration from the https://github.com/fjb4/steeltoe-config-repo repository.

## How to Run with Docker (local)
- Prerequisites
  - [.NET Core SDK](https://dotnet.microsoft.com/download) (tested with version 2.2)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop) (tested with 2.1)
- Start Steeltoe services
  - `docker-compose up -d`
- Stop Steeltoe services
  - `docker-compose down`
- Run
  - Open a terminal window and move to the "FakeNewsService" directory
    - `dotnet watch run`
  - Open a second terminal window and move to the "FakeNewsWeb" directory
    - Set the "BUILD" environment variable to have the value "LOCAL"
      - Unix Bash: `export BUILD=LOCAL`
      - Windows CMD: `set BUILD=LOCAL`
    - `dotnet watch run`
  - View the Fake News web site at `http://localhost:5000`
- Steeltoe service URLs
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
- Goal
  - Demonstration of using some Steeltoe features
- Give brief overview of Fake News website & review its implementation
  - Show the application running
  - [Frontend](http://fake-news-web.cfapps.io) is ASP.NET Core MVC
    - Show HomeController.Index() and view
      - Pulls values from configuration and uses them in the Razor view
      - Retrieves headlines from a REST endpoint and passes them to the view
  - [Backend](http://fake-news-service.cfapps.io/api/headline) is ASP.NET Core WebApi
    - Provides endpoint that returns a collection of randomly selected headlines
- [Spring Config Provider](https://steeltoe.io/app-configuration/get-started/springconfig) Demonstration
  - Utilizes Spring Cloud Config Server
    - Config Server itself must be configured to tell it where to pull configuration data
      - Show config-server.json file
  - Show the [configuration repo](https://github.com/fjb4/steeltoe-config-repo)
    - Application's name determines which YAML file is used
  - Code should look familiar; retrieving configuration values uses standard .NET configuration API
    - Code is no different than retrieving config settings from an appSettings.json file
- [Service Discovery](https://steeltoe.io/service-discovery/get-started/eureka) Demonstration
  - How are the different service instances able to communicate?
    - Clients don't know fully qualified URL of other services
  - Show service registry with list of registered apps
    - This is the "phone book" that allows apps to lookup a service's URL
  - Show code changes necessary to implement service discovery
    - HttpClient has been augmented with DiscoveryHttpClientHandler in GetHeadlinesCommand class
  - Run multiple instances of the backend service
    - `cf scale fake-news-service -i 3`
  - Wait for additional service instances to appear in service registry
  - Calls to backend service are load balanced across the multiple service instances
- [Circuit Breaker](https://steeltoe.io/circuit-breakers/get-started/breaker) Demonstration
  - Show the Circuit Breaker Dashboard
    - Note that all the circuits are closed
  - Show code changes needed to implement circuit breaker
    - GetHeadlinesCommand class that derives from HystrixCommand
      - Discuss RunAsync() vs RunFallbackAsync()
  - Demonstrate a circuit opening
    - Kill one of the services: `cf stop fake-news-service`
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
    - Restart the service that was killed: `cf start fake-news-service`
    - Refresh the frontend service until backend circuit closes again
    - Dashboard shows the circuit is now open again
    - Service is restored, each call to backend again calls RunAsync()
- Steeltoe is a collection of tools for building .NET microservices
  - Features & documentation available at https://steeltoe.io
  - Start a Steeltoe project at https://start.steeltoe.io
  - Able to run in the cloud or locally, using Docker
  - This demonstration is available at https://github.com/fjb4/steeltoe-demo

### Notes
- Circuit breaker configuration settings
  - requestVolumeThreshold: Minimum number of requests in a rolling window that will trip the circuit (20)
  - sleepWindowInMilliseconds: Amount of time, after tripping the circuit, to reject requests before allowing attempts again (5000)
  - errorThresholdPercentage: Error percentage at or above which the circuit should trip open and start short-circuiting requests to fallback logic (50)
