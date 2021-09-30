# Begin Development

If you choose to use VS Code you can use the development containers instead of installing everything locally on your machine.
However if you prefer you can also install everything locally for development.
The solution has been setup to support `docker-compose` which will mimic the production infrastructure implementation.

## Required Installation

- [Git](https://git-scm.com/downloads)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

> If using Windows OS it is preferable to **not** use the WSL version of Docker Desktop. While it may work, mileage will vary in certain scenarios.

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
In theory you will no longer need to worry about a forgotten or missing dependency, or an OS related issue.

Development containers provide an amazing way to ensure all developers have the identical environment to work in.

VS Code - [more information here](https://code.visualstudio.com/docs/remote/containers)

- [VS Code](https://code.visualstudio.com/download)

After installing vscode will recognize if you open a folder that contains a `.devcontainer` folder and ask you if you want to open the container.
Opening the container will spin up a container with all the required dependencies to do development work locally.

## Local Development

To setup your local environment to run and develop the TNO solution, download or clone this mono-repo [https://github.com/bcgov/tno](https://github.com/bcgov/tno).

You will need to install the following dependencies if you do not plan to use the **Development Containers** (above).

| Dependency | Version  |
| ---------- | -------- |
| Node       | v16.10.0 |
| npm        | 7.24.0   |
| yarn       | v1.22.5  |
| jdk        | 11       |
| maven      | 3.8.2    |

## Configure Environment

In order for the various components of the solution to work they require the appropriate configuration files to be created.
You can auto generate them with the provided scripts.
This process will generate `.env` files in the required locations so that the docker containers can run.

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

| Container  | Port  | Description                                                                                   |
| ---------- | ----- | --------------------------------------------------------------------------------------------- |
| nginx      | 50080 | Provides a reverse proxy network configuration enable a single entry point to the application |
| keycloak   | 50000 | Provides authentication and account management services                                       |
| database   | 50002 | Provides relational database for the API                                                      |
| api-editor | 50003 | Provides the RESTful API which gives secure access to data                                    |
| app-editor | 50005 | Provides the web application which is the UI                                                  |

If you have installed `make` you can use the helper method.

```bash
make up
```

If you haven't installed `make` you can use the docker-compose cli.

```bash
docker-compose up
```

Once everything is up and running you can now view the application in your browser.
Login into Keycloak with the username and password you configured in your `.env` file.
You can then change the passwords for the default users to anything you would like.
Then you can login to the web application with one of those default users.

Be aware most of the api endpoints will require a valid JWToken.
Use Postman to interact with the API independently from the web application.
Read more [here](../test/README.md).

| Container  | URI                                                      |
| ---------- | -------------------------------------------------------- |
| keycloak   | [http://localhost:50000/](http://localhost:50000)        |
| app-editor | [http://localhost:50080/app](http://localhost:50080/app) |
| app-api    | [http://localhost:50080/api](http://localhost:50080/api) |
