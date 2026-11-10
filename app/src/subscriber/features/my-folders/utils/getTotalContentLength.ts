import { IFolderModel } from '@/shared';

export const getTotalContentLength = (folders: IFolderModel[]): number => {
  return folders.reduce((total, folder) => total + folder.content.length, 0);
};
