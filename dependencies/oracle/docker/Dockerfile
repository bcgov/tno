# FROM container-registry.oracle.com/database/free:latest
# FROM container-registry.oracle.com/database/free:19.3.0.0
# FROM doctorkirk/oracle-19c:19.9

# works, but slow to build
FROM container-registry.oracle.com/database/enterprise:19.3.0.0

# Logically we want these to be 'setup' scripts which only get run once
# but the oracle script only seems to run those if you are setting up a db from scratch.
# The workaround here is that the scripts reset the db every time now.
# COPY ./scripts /opt/oracle/scripts/startup

