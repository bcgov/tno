# Services

The services have a shared ConfigMap, you will need to create a `config.env` file and place it in the environment overlay (i.e. `/syndication/overlays/dev/config.env`).

```env
API_HOST_URL=http://api:8080

KEYCLOAK_AUTH_SERVER_URL=https://dev.oidc.gov.bc.ca/auth
KEYCLOAK_REALM=gcpe
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET=${ValueFromKeycloak}

KAFKA_BOOTSTRAP_SERVERS=kafka-broker-0.kafka-headless:9092,kafka-broker-1.kafka-headless:9092,kafka-broker-2.kafka-headless:9092
```
