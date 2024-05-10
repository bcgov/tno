import { Request, Response } from 'express';
import { ChartCanvas, ChartTypes } from '../charts';
import {
  convertBase64ConfigToChartJsConfig,
  convertChartJsConfigToBase64String,
  evalScripts,
  getInt,
  getString,
} from '.';
import { ChartConfiguration, ScriptableContext } from 'chart.js';
import { AnyObject } from 'chart.js/dist/types/basic';

/**
 * Generate a chart on a canvas.
 * @param type Chart.js graph type.
 * @param req HTTP request.
 * @returns A ChartCanvas object.
 */
export function generateCanvas(type: ChartTypes, req: Request) {
  const width = getInt(req, 'width') ?? 600;
  const height = getInt(req, 'height') ?? 400;

  let data: any;
  if (req.method === 'GET') {
    const base64Data = getString(req, 'data') ?? convertChartJsConfigToBase64String({});
    data = convertBase64ConfigToChartJsConfig(base64Data);
  } else if (['POST', 'PUT', 'PATCH'].includes(req.method)) {
    data = req.body;
  }

  const base64Options = getString(req, 'options') ?? convertChartJsConfigToBase64String({});
  const options = convertBase64ConfigToChartJsConfig(base64Options);

  const config = { type, data: evalScripts(data), options } as ChartConfiguration;

  const canvas = new ChartCanvas(width, height);
  canvas.applyChart(config);
  return canvas;
}

/**
 * Generate a chart on a canvas and convert it to a base64 string that represents an image.
 * @param type Chart.js graph type.
 * @param req HTTP request.
 * @returns Base64 string.
 */
export function generateBase64(type: ChartTypes, req: Request) {
  const canvas = generateCanvas(type, req);
  const base64 = canvas.toDataURL();
  canvas.destroy();
  return base64;
}

/**
 * Generate a chart on a canvas and convert it to a ChartImage object which contains the image as binary data.
 * @param type Chart.js graph type.
 * @param req HTTP request.
 * @returns ChartImage
 */
export function generateChartImage(type: ChartTypes, req: Request) {
  const canvas = generateCanvas(type, req);
  const dataURL = canvas.toDataURL();
  canvas.destroy();
  return canvas.toImageData(dataURL);
}

/**
 *
 * @param type Chart.js graph type.
 * @param req HTTP request.
 * @param res HTTP response.
 * @returns Response containing image of graph.
 */
export function sendImage(type: ChartTypes, req: Request, res: Response) {
  const chart = generateChartImage(type, req);
  res.writeHead(200, {
    'Content-Type': chart.mimetype,
    'Content-Length': chart.bytes.length,
  });
  return res.end(chart.bytes);
}
