const audioExtensions = [
  'mp3',
  'wav',
  'aac',
  'm4a',
  'm4p',
  'raw',
  'wma',
  'msv',
  'aa',
  'aax',
  'act',
];

/**
 * Function used to check if a given file is an audio.
 * @param fileName The file name
 * @returns True if audio file has an audio extension.
 */
export const isAudioFile = (fileName: string) => {
  return audioExtensions.some((v) => fileName.endsWith(`.${v}`));
};
