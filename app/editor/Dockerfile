FROM node:18-bullseye-slim

# RUN apt-get update && \
#   apt-get install nano

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN apt-get update
RUN apt-get install -y build-essential libcairo2-dev libpango1.0-dev libjpeg-dev libgif-dev librsvg2-dev

RUN yarn set version 3.x
RUN npx update-browserslist-db@latest
RUN yarn
COPY . .

EXPOSE 3000

CMD ["yarn", "start"]
