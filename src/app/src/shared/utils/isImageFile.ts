const imageExtensions = [
  'jpg',
  'jpeg',
  'gif',
  'png',
  'heif',
  'svg',
  'eps',
  'webp',
  'tiff',
  'pdf',
  'psd',
  'ai',
  'raw',
];

/**
 * Function used to check if a given file is an image.
 * @param fileName The file name
 * @returns True if image file has an image extension.
 */
export const isImageFile = (fileName: string) => {
  return imageExtensions.some((v) => fileName.endsWith(`.${v}`));
};
