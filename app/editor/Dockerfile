FROM node:16-bullseye

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN yarn set version berry
RUN yarn
COPY . .

EXPOSE 3000

CMD ["yarn", "start"]
