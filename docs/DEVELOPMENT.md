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

VS Code - [more information here](https://code.visualstudio.com/docs/remote/containers)

- [VS Code](https://code.visualstudio.com/download)

After installing vscode will recognize if you open a folder that contains a `.devcontainer` folder and ask you if you want to open the container.
Opening the container will spin up a container with all the required dependencies to do development work locally.

## Local Development

To setup your local environment to run and develop the TNO solution, download or clone this mono-repo [https://github.com/bcgov/tno](https://github.com/bcgov/tno).

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
