FROM maven:3.8.2-jdk-11 AS base
WORKDIR /project
COPY . .
WORKDIR /project
RUN mvn package

FROM openjdk:11-jdk-slim AS final
WORKDIR /app
COPY --from=base /project/target .

EXPOSE 8080

ENTRYPOINT ["java","-jar","ApiEditor.jar"]
