# Sonarqube

To install Sonarqube.
You will need `.env` files for parameters if you are not using the default values.

## Installation

```bash
# Login
oc login --token=${token} --server=https://api.silver.devops.gov.bc.ca:6443

# Go to your TOOLS project
oc project ${project:-9b301c-tools}

# Deploy a postgres database in the tools namespace.
oc process -f postgres/secrets.yaml | oc create --save-config=true -f -
oc process -f postgres/pvc.yaml | oc create --save-config=true -f -
oc process -f postgres/deploy.yaml | oc create --save-config=true -f -

# Deploy sonarqube in the tools namespace.
oc process -f secrets.yaml | oc create --save-config=true -f -
oc process -f pvc.yaml | oc create --save-config=true -f -
oc process -f deploy.yaml | oc create --save-config=true -f -

# Create a route to connect to sonarqube.
oc process -f route.yaml | oc create --save-config=true -f -
```

View the app [https://tno-sonarqube.apps.silver.devops.gov.bc.ca](https://tno-sonarqube.apps.silver.devops.gov.bc.ca)

Use the sonarqube default username:admin and password:admin when you login the first time.
Change the password with the one stored in the sonarqube secrets.
