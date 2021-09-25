FROM node:16.10

WORKDIR /usr/app
COPY package*.json yarn.lock ./

RUN yarn
COPY . .

EXPOSE 3000

CMD ["yarn", "start"]
