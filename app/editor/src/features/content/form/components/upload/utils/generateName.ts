import { IFile } from '..';
import { calcSize } from '.';

export const generateName = (file?: IFile): string | undefined => {
  if (!file) return undefined;

  var size = calcSize(file?.size);
  return `${file.name}${!!size ? ` (${size})` : ''}`;
};
