import { type IFile } from '..';
import { calcSize } from '.';

export const generateName = (file?: IFile) => {
  if (!file) return undefined;

  const size = calcSize(file?.size);
  return `${file.name}${size ? ` (${size})` : ''}`;
};
