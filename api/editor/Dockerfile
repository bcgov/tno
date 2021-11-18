FROM maven:3.6.3-openjdk-17 AS base
WORKDIR /project
COPY . .
WORKDIR /project/api-editor
RUN mvn package

FROM openjdk:17.0.1-slim AS final
WORKDIR /app
COPY --from=base /project/api-editor/target .

RUN apt-get update && apt-get install curl -y

EXPOSE 8080

ENTRYPOINT ["java","-jar","ApiEditor.jar"]
