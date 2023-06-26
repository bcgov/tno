import express from 'express';
import routes from './routes';
import favicon from 'serve-favicon';
import path from 'path';

class App {
  public server;

  constructor() {
    this.server = express();

    this.middlewares();
    this.routes();
  }

  middlewares() {
    this.server.use(express.json());
  }

  routes() {
    this.server.use(favicon(path.join(__dirname, 'public/images', 'favicon.ico')));
    this.server.use(routes);
  }
}

export default new App().server;
