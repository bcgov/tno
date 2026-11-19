# TNO API

The .NET REST API lives in `src/api`. From the repository root:

```bash
dotnet build src/api/TNO.API.csproj
bash do docker up -n api
```

For manual API testing, import [TNO.postman_collection.json](TNO.postman_collection.json)
and follow the [Postman guide](../../docs/POSTMAN.md) to configure the environment,
request a Keycloak access token, and send authenticated requests.
Automated API tests live in [src/tests/api](../tests/api/README.md).
