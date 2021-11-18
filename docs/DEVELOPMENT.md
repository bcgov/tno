# Begin Development

Development can be performed by any IDE that supports Java and React. However, this repositority is specifically built to support VS Code. VS Code will enable developers to get everything running significantly quicker.

If you choose to use VS Code you can use the development containers instead of installing everything locally on your machine.
The solution has been setup to support `docker-compose` which will mimic the production infrastructure implementation.

To setup your local environment to run and develop the TNO solution, download or clone this mono-repo [https://github.com/bcgov/tno](https://github.com/bcgov/tno).

## Docker Development

If you choose to run the solution with Docker you will need to install the following.

- [Docker Desktop](https://www.docker.com/products/docker-desktop)

> If using Windows OS it is preferable to **not** use the WSL version of Docker Desktop. While it may work, mileage may vary in certain scenarios.

There are a number of Windows related dependencies that are required to take advantage of some of the automated build scripts.
These scripts simplify the process of setting up your environment, but can be performed manually if you do not want to install the following.

| Windows                                                | Mac                                                     | Description                                                                       |
| ------------------------------------------------------ | ------------------------------------------------------- | --------------------------------------------------------------------------------- |
| [Git Bash](https://git-scm.com/)                       |                                                         | All scripts in this project are `bash` which natively isn't supported by Windows. |
| [chocolately](https://chocolatey.org/install)          |                                                         | Package manager for Windows.                                                      |
| [make](https://community.chocolatey.org/packages/make) | [make](https://formulae.brew.sh/formula/make)           | Useful tool for speeding up common activities.                                    |
|                                                        | [coreutils](https://formulae.brew.sh/formula/coreutils) |

## Development Containers

VS Code supports developing inside a container.
This provides a very quick and reliable way to setup your whole local development environment without installing anything locally.
In theory you will no longer need to worry about missing dependencies, correct versions, or OS related issues.

Development containers provide an amazing way to ensure all developers have the identical environment to work in.

VS Code - [more information here](https://code.visualstudio.com/docs/remote/containers)

- Install [VS Code](https://code.visualstudio.com/download)

After installing vscode will recognize when you open a folder that contains a `.devcontainer` folder and ask you if you want to open the container.
Choosing to open the container will spin up one with all the required dependencies to do development work locally.
In addition, it will also install all the associated plugins for VS Code.

## Local Development (Without Development Containers)

If you would like to be able to build the solution locally or run the various components locally (not with Docker), you will need to install the following dependencies.
This can be helpful if your computer's performance is unable to support development within docker containers.

| Dependency                                                                                                                                           |  Version | Description                                                |
| ---------------------------------------------------------------------------------------------------------------------------------------------------- | -------: | ---------------------------------------------------------- |
| [Node](https://nodejs.org/en/download/)                                                                                                              | v16.10.0 |                                                            |
| [nvm](https://github.com/coreybutler/nvm-windows#node-version-manager-nvm-for-windows)                                                               |          | Required if you need to support different versions of node |
| [npm](https://docs.npmjs.com/cli/v7/configuring-npm/install)                                                                                         |   7.24.0 |                                                            |
| [yarn](https://classic.yarnpkg.com/en/docs/install/#windows-stable)                                                                                  |  v1.22.5 |                                                            |
| [jdk](https://docs.oracle.com/en/java/javase/11/install/installation-jdk-microsoft-windows-platforms.html#GUID-A7E27B90-A28D-4237-9383-A58B416071CA) |       11 |                                                            |
| [maven](http://maven.apache.org/install.html)                                                                                                        |    3.8.2 |                                                            |

## Configure Environment

In order for the various components of the solution to work they require the appropriate configuration files to be created.
You can auto generate them with the provided scripts.
This process will generate `.env` files in the required locations so that the solution and docker containers can run.

If you have installed `make` you can use the helper method.

```bash
make setup
```

If you haven't installed `make` you can manually run the bash script.

```bash
./tools/scripts/gen-env-files.sh
```

## Standup Local Environment

You now have everything installed and ready to be run locally.
All that remains is to turn everything on.
The first time you do this takes a little longer as each container needs to be built and initialized.
After the docker containers are ready it becomes much quicker.

The following containers are hosted in the TNO solution.
The exposed container ports is configurable, but the defaults are identified below.

| Container       |                Port | Description                                                                                   |
| --------------- | ------------------: | --------------------------------------------------------------------------------------------- |
| nginx           |               50080 | Provides a reverse proxy network configuration enable a single entry point to the application |
| keycloak        |               50000 | Provides authentication and account management services                                       |
| database        |               50002 | Provides PostgreSQL relational database for the API                                           |
| elastic         |               50007 | Provides NoSQL Elasticsearch database for the API                                             |
| azure-storage   | 50020, 50021, 50022 | Azurite local Azure Storage for development                                                   |
| api-editor      |               50003 | Provides the RESTful API which gives secure access to data                                    |
| app-editor      |               50005 | Web application for Editors                                                                   |
| app-subscriber  |               50050 | Web application for Subscribers                                                               |
| zookeeper       |               50010 | Kafka Zookeeper to manage cluster                                                             |
| broker          |        50012, 50017 | Kafka server and REST API v3                                                                  |
| rest-proxy      |               50018 | Kafka REST API                                                                                |
| schema-registry |               50013 | Kafka schema registry services                                                                |
| connect         |               50014 | Kafka connect Control Center with Schema Registry                                             |
| ksqldb-server   |               50016 | Kafka streaming services                                                                      |

If you have installed `make` you can use the helper method.

```bash
make up
```

If you haven't installed `make` you can use the docker-compose cli.
The following command merges multiple compose files, builds and runs all of the containers.
The assumption for the rest of the documentation is that you have installed `make`.

```bash
docker-compose -f docker-compose.override.yml -f docker-compose.yml -f ./db/kafka/docker-compose.yml -d up
```

Once these containers are running you can then start up the other services.

```bash
make service-up
```

| Container  |  Port | Description                                                    |
| ---------- | ----: | -------------------------------------------------------------- |
| kafka kowl | 50017 | Kafka UI to view cluster                                       |
| atom       |       | Kafka Producer to ingest syndication ATOM feeds                |
| rss        |       | Kafka Producer to ingest syndication RSS feeds                 |
| transcribe |       | Kafka Consumer/Producer to transcribe audio/video content      |
| nlp        |       | Kafka Consumer/Producer to perform Natural Language Processing |
| index      |       | Kafka Consumer to index content for search                     |

Once everything is up and running you can now view the application in your browser.
Login into Keycloak with the username and password you configured in your `.env` file.
You can then change the passwords for the default users to anything you would like.
Then you can login to the web application with one of those default users.

Be aware most of the api endpoints will require a valid JWToken.
Use Postman to interact with the API independently from the web application.
Read more [here](../test/README.md).

| Container        | URI                                                      |
| ---------------- | -------------------------------------------------------- |
| keycloak         | [http://localhost:50000/](http://localhost:50000)        |
| app-editor       | [http://localhost:50080/app](http://localhost:50080/app) |
| app-subscriber   | [http://localhost:50080](http://localhost:50080)         |
| app-api          | [http://localhost:50080/api](http://localhost:50080/api) |
| elastic          | [http://localhost:50007](http://localhost:50007)         |
| kafka kowl       | [http://localhost:50017](http://localhost:50017)         |
| kafka rest-proxy | [http://localhost:50018](http://localhost:50018)         |
