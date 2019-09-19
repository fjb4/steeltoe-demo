# Cloud Foundry Setup
- cf create-service p-config-server trial myConfigServer -c ./config-server.json
    - cf bind-service myApp myConfigServer
- cf create-service p-service-registry trial myDiscoveryService
    - cf bind-service myApp myDiscoveryService


# Local Setup
- docker container run -p 8761:8761 --name eureka-server --rm -d steeltoeoss/eurekaserver
- docker container run -p 8888:8888 --name config-server --rm -d steeltoeoss/config-server --spring.cloud.config.server.git.uri=https://github.com/fjb4/steeltoe-config-repo
