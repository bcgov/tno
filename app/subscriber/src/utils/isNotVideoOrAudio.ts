/**
 * Function used to check if a given file is a video or audio or not.
 * @param fileName The file name
 * @returns True if it is not a video or audio file
 */
export const isNotVideoOrAudio = (fileName: string) => {
  return fileName.endsWith('.jpg') || fileName.endsWith('.xml');
};
