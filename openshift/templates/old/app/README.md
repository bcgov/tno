# React Application

To install the React web application as a production build with Nginx server.
You will need `.env` files for parameters if you are not using the default values.

```bash
# Login
oc login --token=${token} --server=https://api.silver.devops.gov.bc.ca:6443

# Go to your TOOLS project
oc project ${project:-9b301c-tools}

# Create a BuildConfig that will generate an image from source code.
oc process -f build.yaml | oc create --save-config=true -f -

# Create a DeployConfig and Service to deploy and access the app.
oc process -f deploy.yaml | oc create --save-config=true -f -

# Create a route to connect to app service
oc process -f route.yaml | oc create --save-config=true -f -
```

View the app [https://tno-dev.apps.silver.devops.gov.bc.ca](https://tno-dev.apps.silver.devops.gov.bc.ca)
