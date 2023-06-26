import { Router } from 'express';
import { CategoryScale, Colors } from 'chart.js';
import Chart from 'chart.js/auto';
import { getString, convertChartJsConfigToBase64String, generateBase64, sendImage } from './utils';
import { ChartTypes } from './charts';
import { data } from './data/data';

Chart.register(CategoryScale);
Chart.defaults.animation = false;
Chart.defaults.responsive = false;
Chart.defaults.maintainAspectRatio = true;
Chart.register(Colors);

const routes = Router();

/** Temporary endpoint to provide test data. */
routes.get('/data', (req, res) => {
  const base64 = convertChartJsConfigToBase64String(data);
  res.send({ data: base64 });
});

/** Draw a chart on a canvas and return an image. */
routes.get('/graph', async (req, res) => {
  const type = (getString(req, 'type') as ChartTypes) ?? ChartTypes.bar;
  return sendImage(type, req, res);
});

/** Draw a chart on a canvas and return an image. */
routes.get('/graph/:type', (req, res) => {
  const type = req.params.type as ChartTypes;
  return sendImage(type, req, res);
});

/** Draw a chart on a canvas and return a base64 string */
routes.get('/base64', async (req, res) => {
  const type = (getString(req, 'type') as ChartTypes) ?? ChartTypes.bar;
  return res.send(generateBase64(type, req));
});

/** Draw a chart on a canvas and return a base64 string */
routes.get('/base64/:type', (req, res) => {
  const type = req.params.type as ChartTypes;
  return res.send(generateBase64(type, req));
});

export default routes;
