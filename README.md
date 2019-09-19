Setup
- cf create-service p-config-server trial myConfigServer -c ./config-server.json
- cf bind-service myApp myConfigServer

- cf create-service p-service-registry trial myDiscoveryService
- cf bind-service myApp myDiscoveryService

 