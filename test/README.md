# Testing

Each project will have its own unit tests, however 3rd party testing tools are also used, such as Postman.
Postman provides a way to send HTTP requests to endpoints and validate the results.

## Postman

Import the `./test/TNO.postman_collection.json` file into your Postman. Create an environment variables collection with the following keys.

| Key                          | Default Value | Description                                                                |
| ---------------------------- | ------------- | -------------------------------------------------------------------------- |
| keycloak-scheme              | http          | Uri scheme to connect to Keycloak [http\|https]                            |
| keycloak-host                | localhost     | Uri domain host to connect to Keycloak [localhost \| host.docker.internal] |
| keycloak-port                | 40001         | Uri port to connect to Keycloak                                            |
| realm                        | tno           | Keycloak realm for application                                             |
| service-account-secret       | {key}         | Keycloak Secret key to authenticate service account                        |
| schema                       | http          | Uri scheme to connect to API [http\|https]                                 |
| host                         | localhost     | Uri domain host to connect to API                                          |
| port                         | 40010         | Uri port to connect to API                                                 |
| test-username                | admin         | Username for test account                                                  |
| test-password                | {password}    | Password for test account                                                  |
| test-secret                  | {key}         | Keycloak secret key to authenticate the test account                       |
| root-path                    | /api          | API default root path. Resolves reverse proxy vs direct                    |
| azure-video-location         | trail         | Azure Video location                                                       |
| azure-video-account-id       |               | Azure Video Analyzer account id                                            |
| azure-video-subscription-key |               | Azure Video Analyzer subscription key                                      |
| kafka-rest-port              | 40104         | Port to the Kafka REST proxy                                               |
| nlp-port                     | 40022         | Port to the NLP service API                                                |

The Postman collection has an `TNO/auth` folder that contains endpoints that connect to Keycloak.
Use one of the `token: {name}` endpoints to get a valid authentication token from Keycloak.
This token will then automatically be included in all other requests in the collection.
