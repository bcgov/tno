# .NET tests

The test projects live in `src/tests`: `ai`, `api`, `core`, `dal`, `entities`, and
`services`. Run these commands from the repository root with the repository's .NET 10 SDK:

```bash
dotnet test TNO.sln --configuration Release
# Run one project:
dotnet test src/tests/api/TNO.Test.API.csproj --configuration Release
```

Integration tests require their dedicated services and opt-in environment variables.
Skipped integration tests are not passing coverage. See the
[API integration test instructions](api/README.md) and each suite's fixture setup
before running against a disposable test database or Elasticsearch instance.

## Coverage

Generate coverage for a project with its checked-in Coverlet settings:

```bash
dotnet test src/tests/dal/TNO.Test.DAL.csproj --configuration Release \
  --collect:"XPlat Code Coverage" --settings src/tests/dal/coverlet.runsettings \
  --results-directory src/tests/TestResults
```

The collector writes results below a run-specific directory in `src/tests/TestResults`.
The `net-libs.yml` CI workflow gathers coverage from the DAL, entity, and service suites.
To render the generated OpenCover reports locally:

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator '-reports:src/tests/TestResults/**/coverage.opencover.xml' \
  '-targetdir:src/tests/TestResults/Coverage' '-reporttypes:Html'
```

Open `src/tests/TestResults/Coverage/index.html` in a browser.
For manual API requests, see the [Postman guide](../../docs/POSTMAN.md).
