# Testing

Each project will have its own unit tests, however 3rd party testing tools are also used, such as Postman.
Postman provides a way to send HTTP requests to endpoints and validate the results.

## Postman

Import the `./test/TNO.postman_collection.json` file into your Postman. Create an environment variables collection with the following keys.

| Key                    | Value | Description                                          |
| ---------------------- | ----- | ---------------------------------------------------- |
| keycloak-scheme        |       | Uri scheme to connect to Keycloak [http\|https]      |
| keycloak-host          |       | Uri domain host to connect to Keycloak               |
| keycloak-port          |       | Uri port to connect to Keycloak                      |
| realm                  |       | Keycloak realm for application                       |
| service-account-secret |       | Keycloak Secret key to authenticate service account  |
| schema                 |       | Uri scheme to connect to API [http\|https]           |
| host                   |       | Uri domain host to connect to API                    |
| port                   |       | Uri port to connect to API                           |
| test-username          |       | Username for test account                            |
| test-password          |       | Password for test account                            |
| test-secret            |       | Keycloak secret key to authenticate the test account |

The Postman collection has an `TNO/auth` folder that contains endpoints that connect to Keycloak.
Use one of the `token: {name}` endpoints to get a valid authentication token from Keycloak.
This token will then automatically be included in all other requests in the collection.
