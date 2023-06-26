export type ChartImage = {
  /**
   * Mimetype of the image.
   */
  mimetype: string;
  /**
   * Base64 string representing the image.
   */
  dataURL: string;
  /**
   * Raw byte data of image.
   */
  bytes: Uint8Array;
};
