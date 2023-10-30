FROM node:18-bullseye-slim

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN yarn set version 3.x
RUN npx update-browserslist-db@latest
RUN yarn
COPY . .

EXPOSE 3000

CMD ["yarn", "start"]
