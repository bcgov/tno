import { IFolderModel } from 'tno-core';

/**
 * Sorts the folders.
 * @param folders An array of folders.
 * @returns A sorted array of folders.
 */
export const sortFolders = (folders: IFolderModel[]) => {
  return [...folders].sort((a, b) => {
    if (a.sortOrder === b.sortOrder) return a.name < b.name ? -1 : a.name > b.name ? 1 : 0;
    return a.sortOrder < b.sortOrder ? -1 : a.sortOrder > b.sortOrder ? 1 : 0;
  });
};
