import { isAudioFile, isVideoFile } from '.';

/**
 * Function used to check if a given file is a video or audio or not.
 * @param fileName The file name
 * @returns True if it is not a video or audio file
 */
export const isVideoOrAudioFile = (fileName: string) => {
  return isAudioFile(fileName) || isVideoFile(fileName);
};
