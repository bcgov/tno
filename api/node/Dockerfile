FROM node:18-bullseye AS build

WORKDIR /usr/app
COPY .yarn ./.yarn
COPY package*.json yarn.lock .yarnrc.yml ./

RUN yarn set version berry
RUN npx browserslist@latest --update-db
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
