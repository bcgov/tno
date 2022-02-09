FROM maven:3.6.3-openjdk-17 AS base

WORKDIR /project
COPY . .
RUN mvn package

FROM openjdk:17.0.1-slim AS final

WORKDIR /app
COPY --from=base /project/target .

ENTRYPOINT ["java","-jar","/app/DbMigration.jar"]

# ENTRYPOINT ["tail"]
# CMD ["-f","/dev/null"]
