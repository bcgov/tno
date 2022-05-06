FROM maven:3.6.3-openjdk-17 AS base
WORKDIR /project
COPY . .

RUN mvn clean
RUN mvn package

FROM openjdk:17.0.1-slim AS final
WORKDIR /app
COPY --from=base /project/target .

RUN apt-get update && apt-get install curl -y && apt-get install ffmpeg -y

EXPOSE 8080

ENTRYPOINT ["java","-jar","AudioService.jar"]

# ENTRYPOINT ["tail"]
# CMD ["-f","/dev/null"]
