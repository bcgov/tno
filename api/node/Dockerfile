FROM node:18-bullseye AS build

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN apt-get update
RUN apt-get install -y build-essential libcairo2-dev libpango1.0-dev libjpeg-dev libgif-dev librsvg2-dev

RUN yarn set version 3.x
RUN npx update-browserslist-db@latest
RUN yarn
COPY . .
RUN yarn run build

FROM node:18-bullseye AS final

WORKDIR /usr/app
COPY --from=build /usr/app/node_modules ./node_modules
COPY --from=build /usr/app/build/ .

EXPOSE 8080

CMD ["node", "dist/server.js"]
# ENTRYPOINT ["tail", "-f", "/dev/null"]
