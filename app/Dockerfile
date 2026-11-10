# Development server used by docker-compose (services `editor` and `subscriber`).
# Both services run this image; APP_MODE (editor|subscriber) selects the design.
# src/, public/ and the HTML documents are bind-mounted, so code changes hot-reload.
FROM node:26-bookworm-slim

WORKDIR /usr/app

COPY package.json package-lock.json .npmrc ./
COPY packages ./packages
RUN npm ci --no-audit --no-fund

COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev"]
