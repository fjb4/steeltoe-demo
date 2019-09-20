# Run on Docker (local)
- `docker-compose up -d`
- `docker-compose down`
- `export BUILD=LOCAL` (make sure this is unset to run on Cloud Foundry)
- `dotnet watch run --launch-profile=backend`
- `dotnet watch run --launch-profile=middleware`
- `dotnet watch run --launch-profile=frontend`


# Run on Cloud Foundry
- `cf create-service p-config-server trial myConfigServer -c ./config-server.json`
- `cf create-service p-service-registry trial myDiscoveryService`
- `cf create-service p-circuit-breaker-dashboard trial myHystrixService`
- `cf push`
