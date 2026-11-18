const videoExtensions = [
  'mp4',
  'webm',
  'mkv',
  'flv',
  'gif',
  'mng',
  'avi',
  'mov',
  'wmv',
  'yuv',
  'rm',
  'mpg',
  'mpg2',
  'mpeg',
  'mpe',
  'mpv',
  'm2v',
  'm4v',
];

/**
 * Function used to check if a given file is a video.
 * @param fileName The file name
 * @returns True if video file has a video extension.
 */
export const isVideoFile = (fileName: string) => {
  return videoExtensions.some((v) => fileName.endsWith(`.${v}`));
};
