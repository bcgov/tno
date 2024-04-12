import { IFolderModel } from 'tno-core';

export const getTotalContentLength = (folders: IFolderModel[]): number => {
  return folders.reduce((total, folder) => total + folder.content.length, 0);
};
