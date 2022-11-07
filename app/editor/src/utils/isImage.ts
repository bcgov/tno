/**
 * Function used to check if a given file is an image or not.
 * @param fileName The file name
 * @returns True if it is an image file
 */
export const isImage = (fileName: string) => {
  return fileName.endsWith('.jpg');
};
