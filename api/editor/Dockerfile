FROM maven:3.8.2-jdk-11 AS base
WORKDIR /project
COPY . .
WORKDIR /project/api-editor
RUN mvn package

FROM openjdk:11-jdk-slim AS final
WORKDIR /app
COPY --from=base /project/api-editor/target .

EXPOSE 8080

ENTRYPOINT ["java","-jar","api-editor-0.0.1-alpha.jar"]
