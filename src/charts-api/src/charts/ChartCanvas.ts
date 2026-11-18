import { Canvas, JpegConfig, createCanvas } from 'canvas';
import { Chart, ChartConfiguration } from 'chart.js';
import { ChartImage } from '.';

type DataURLCallback = (err: Error | null, result: string) => void;

/**
 * ChartCanvas class, provides a way to create a server side canvas and apply the Chart.js graph to it.
 */
export class ChartCanvas {
  canvas: Canvas;
  chart: Chart | undefined;

  /**
   * Create a new instance of a ChartCanvas object, initializes with specified parameters.
   * @param width Horizontal width in pixels of canvas.
   * @param height Vertical height in pixels of canvas.
   * @param type The type of canvas [pdf|svg].
   */
  constructor(width: number, height: number, type?: 'pdf' | 'svg') {
    this.canvas = createCanvas(width, height, type);
  }

  /**
   * Apply the chart.js chart to the canvas.
   * @param config Chart configuration options.
   */
  applyChart(config: ChartConfiguration) {
    const ctx = this.canvas.getContext('2d');
    this.chart = new Chart(ctx as any, config);
  }

  /**
   * Convert the canvas to a base64 string value.
   * @returns Base 64 string.
   */
  toDataURL(): string;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetype
   * @returns Base 64 string.
   */
  toDataURL(mimetype: 'image/png'): string;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetype
   * @param quality
   * @returns Base 64 string.
   */
  toDataURL(mimetype: 'image/jpeg', quality?: number): string;

  /**
   * Convert the canvas to a base64 string value.
   * @param cb
   */
  toDataURL(cb: DataURLCallback): void;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetype
   * @param cb
   */
  toDataURL(mimetype: 'image/png', cb: DataURLCallback): void;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetype
   * @param quality
   * @param cb
   */
  toDataURL(mimetype: 'image/jpeg', quality: number, cb: DataURLCallback): void;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetype
   * @param config
   * @param cb
   */
  toDataURL(mimetype: 'image/jpeg', config: JpegConfig, cb: DataURLCallback): void;

  /**
   * Convert the canvas to a base64 string value.
   * @param mimetypeOrCallback
   * @param qualityOrConfigOrCallback
   * @param cb
   * @returns Base 64 string or void.
   */
  toDataURL(
    mimetypeOrCallback?: 'image/jpeg' | 'image/png' | DataURLCallback,
    qualityOrConfigOrCallback?: number | JpegConfig | DataURLCallback,
    cb?: DataURLCallback,
  ): void | string {
    // const dataUrl = chart.toBase64Image();
    if (!mimetypeOrCallback) return this.canvas.toDataURL();
    if (!mimetypeOrCallback && typeof mimetypeOrCallback === 'function')
      this.canvas.toDataURL(mimetypeOrCallback);
    if (mimetypeOrCallback === 'image/png' && typeof qualityOrConfigOrCallback === 'function')
      return this.canvas.toDataURL('image/png', qualityOrConfigOrCallback);
    if (mimetypeOrCallback === 'image/png') return this.canvas.toDataURL('image/png');

    // jpeg
    if (cb !== undefined && typeof qualityOrConfigOrCallback === 'object')
      return this.canvas.toDataURL('image/jpeg', qualityOrConfigOrCallback, cb);
    if (cb !== undefined && typeof qualityOrConfigOrCallback == 'number')
      return this.canvas.toDataURL('image/jpeg', qualityOrConfigOrCallback, cb);
    if (typeof qualityOrConfigOrCallback === 'number')
      return this.canvas.toDataURL('image/jpeg', qualityOrConfigOrCallback);

    return this.canvas.toDataURL('image/jpeg', qualityOrConfigOrCallback as DataURLCallback);
  }

  /**
   * Convert the canvas to a base64 string and raw image.
   * @param dataURL Base64 string value of the canvas.
   * @returns An object containing the raw image.
   */
  toImageData(dataURL: string): ChartImage {
    var arr = dataURL.split(',');
    var mimetype = arr?.[0].match(/:(.*?);/)?.[1] ?? 'image/png';
    const base64Data = atob(arr[arr.length - 1]);
    var n = base64Data.length;
    var bytes = new Uint8Array(n);
    while (n--) {
      bytes[n] = base64Data.charCodeAt(n);
    }
    return {
      mimetype,
      dataURL,
      bytes,
    };
  }

  /**
   * Destroy the chart.js object and release memory.
   */
  destroy() {
    this.chart?.destroy();
  }
}
