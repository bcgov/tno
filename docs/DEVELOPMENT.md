# Begin Development

Development can be performed by any IDE that supports Java and React. However, this repositority is specifically built to support VS Code. VS Code will enable developers to get everything running significantly quicker.

If you choose to use VS Code you can use the development containers instead of installing everything locally on your machine.
The solution has been setup to support `docker-compose` which will mimic the production infrastructure implementation.

To setup your local environment to run and develop the TNO solution, download or clone this mono-repo [https://github.com/bcgov/tno](https://github.com/bcgov/tno).

## Docker Development

If you choose to run the solution with Docker you will need to install the following.

- [Docker Desktop](https://www.docker.com/products/docker-desktop)

> If using Windows OS it is preferable to **not** use the WSL version of Docker Desktop. While it may work, mileage may vary in certain scenarios.
> If using Linux ensure the following is added to your `/etc/hosts`: `172.17.0.1 host.docker.internal` and `172.17.0.1 gateway.docker.internal`

There are a number of Windows related dependencies that are required to take advantage of some of the automated build scripts.
These scripts simplify the process of setting up your environment, but can be performed manually if you do not want to install the following.

| Windows                                                | Mac                                                     | Description                                                                             |
| ------------------------------------------------------ | ------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| [Git Bash](https://git-scm.com/)                       |                                                         | All scripts in this project are `bash` which natively isn't supported by Windows.       |
| [chocolately](https://chocolatey.org/install)          |                                                         | Package manager for Windows.                                                            |
| [make](https://community.chocolatey.org/packages/make) | [make](https://formulae.brew.sh/formula/make)           | Useful tool for speeding up common activities.                                          |
|                                                        | [coreutils](https://formulae.brew.sh/formula/coreutils) |
|                                                        | [grep](https://formulae.brew.sh/formula/grep)           | Macos doesn't natively support GNU Grep, so you need to install it. `brew install grep` |
|                                                        | [gnupg](https://formulae.brew.sh/formula/gnupg)         | Generate signing keys `brew install gpnupg`                                             |

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

| Dependency                                                                             |  Version | Description                                                |
| -------------------------------------------------------------------------------------- | -------: | ---------------------------------------------------------- |
| [Node](https://nodejs.org/en/download/)                                                | v18.19.0 |                                                            |
| [nvm](https://github.com/coreybutler/nvm-windows#node-version-manager-nvm-for-windows) |          | Required if you need to support different versions of node |
| [npm](https://docs.npmjs.com/cli/v7/configuring-npm/install)                           |   10.2.3 |                                                            |
| [yarn](https://classic.yarnpkg.com/en/docs/install/#windows-stable)                    |    3.2.5 |                                                            |

To install the correct version of yarn.

```bash
cd ./app/editor
nvm use 18.19.0
npm install --global yarn
```

## Standup Local Environment

In order for the various components of the solution to work they require the appropriate configuration files to be created.
You can auto generate them with the provided scripts.
This process will generate `.env` files in the required locations so that the solution and docker containers can run.

If you have installed `make` you can use the helper method.
If you haven't installed `make` you can use the docker-compose cli.
Review the `Makefile` and the related scripts `./tools/scripts` to understand how to do this.

When using `make` commands you can review the options by using the following command.

```bash
make
```

The following containers are hosted in the TNO solution.
The exposed container ports is configurable, but the defaults are identified below.

| Container  |         Port | Description                                                                                   |
| ---------- | -----------: | --------------------------------------------------------------------------------------------- |
| nginx      | 40080, 40081 | Provides a reverse proxy network configuration enable a single entry point to the application |
| keycloak   |        40001 | Provides authentication and account management services                                       |
| database   |        40000 | Provides PostgreSQL relational database for the API                                           |
| elastic    |        40003 | Provides NoSQL Elasticsearch database for the API                                             |
| api        |        40010 | Provides the RESTful API which gives secure access to data                                    |
| editor     |        40082 | Web application for Editors                                                                   |
| subscriber |        40083 | Web application for Subscribers                                                               |
| broker     | 40101, 40102 | Kafka server and REST API v3                                                                  |

The first time you do this takes a little longer as each container needs to be built and initialized.
After the docker containers are ready it becomes much quicker.
Additionally, there are a number of configuration settings (usernames, passwords, keys, etc) that are created the first time you execute this script.

Many laptops cannot handle running all containers running at one time, so you may want to use the other `make p=$profile` commands specifically to only start what you need.
Make sure you wait for each command to complete successfully.
Laptops with low memory will need at least 8GB assigned to Docker Desktop.

```bash
# Configure your local environment.
make setup

# Run the PostgreSQL database.
make up n=database

# Run Keycloak
make up n=keycloak

# After keycloak starts go to http://localhost:40001 and login to confirm it has successfully initialized.
# There should be a tno realm.  You can review the predefined user accounts.

# Copy the mmi-service-account credentials secret.
# Copy the mmi-app GUID (get it from the URL path in Keycloak).
# Run command line tool
./tools/scripts/kc-key-update.sh -s {your-mmi-service-account-secret} -id {your-mmi-app-GUID}
# Setup the database
make db-update

# Run Kafka.
make up n=broker

# Setup Kafka topics
make kafka-update

# Run Elasticsearch.
make up n=elastic

# Setup the Elastic indexes
make elastic-update

# Run the API
make up n=api

# Run Indexing service
make up n=indexing

# Run the Editor application
make up n=editor

# Run the Subscriber application
make up n=subscriber

# Run nginx
make up n=nginx
# After the editor application starts go to http://localhost:40080 and login with a predefined keycloak user account.
# After the subscriber application start go to http:/localhost:40081 and login with a predefined keycloak user account.
```

If you choose to only run what you need for specific types of feature development/testing you can use the following commands.
Or if you choose to run everything use the `make up` command.

```bash
# Once the main services are all running you can in the future simply use the following command.
make up p=main
```

You can now view the application in your browser.
Login into Keycloak with the username and password you configured in your `.env` file.
You can then change the passwords for the default users to anything you would like.
Then you can login to the web application with one of those default users.

Be aware most of the api endpoints will require a valid JWToken.
Use Postman to interact with the API independently from the web application.
Read more [here](../test/README.md).

Use the Nginx proxy to use the Editor and Subscriber apps.
Nginx provides a reverse proxy, which handles communication with the API.

| Component          | Container  | URI                                               |
| ------------------ | ---------- | ------------------------------------------------- |
| Nginx - Editor     | nginx      | [http://localhost:40080](http://localhost:40080)  |
| Nginx - Subscriber | nginx      | [http://localhost:40081](http://localhost:40081)  |
| Editor app         | editor     | [http://localhost:40082](http://localhost:40082)  |
| Subscriber app     | subscriber | [http://localhost:40083](http://localhost:40083)  |
| API                | api        | [http://localhost:40010](http://localhost:40010)  |
| Keycloak           | keycloak   | [http://localhost:40001/](http://localhost:40001) |
| Kafka Kowl         | kowl       | [http://localhost:40180](http://localhost:40180)  |
| Elasticsearch      | elastic    | [http://localhost:40003](http://localhost:40003)  |

Once the core containers are running you can then start up the other services and utilities.
Locally you will probably never want everything running at the same time.
Choose the services and utilities you need and run them independently.

```bash
# Start up the various utilities to view Kafka, and Elasticsearch.
make up n=[utility name]

# Start up the various supplementary services for the solution.
make up n=[service name]
```

Below is a list of all the additional services and utilities.

| Container     |  Port | Description                                                    |
| ------------- | ----: | -------------------------------------------------------------- |
| syndication   | 40020 | Kafka Producer to ingest syndication feeds                     |
| command       |       | Runs command line utilities                                    |
| capture       | 40021 | Listens to audio/video streams and captures a file             |
| clip          | 40022 | Creates clips from capture files based on schedule             |
| image         | 40023 | Image ingestion service                                        |
| file          | 40024 | File ingestion service                                         |
| content       | 40025 | Kafka Consumer to create content for Editors                   |
| indexing      | 40026 | Kafka Consumer to index content for search                     |
| transcription | 40027 | Kafka Consumer/Producer to transcribe audio/video content      |
| nlp           | 40028 | Kafka Consumer/Producer to perform Natural Language Processing |

To see the Swagger Document, use this link after setting up your local environment: http://localhost:40010/api-docs/index.html
