FROM node:18-bullseye

# RUN apt-get update && \
#   apt-get install nano

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN yarn set version berry
RUN npx browserslist@latest --update-db
RUN yarn
COPY . .

EXPOSE 8080

CMD ["node", "server.js"]
