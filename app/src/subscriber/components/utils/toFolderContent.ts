import { IContentModel, IFolderContentModel } from '@/shared';

/** transform the content to folder content before sending it to the API */
export const toFolderContent = (content: IContentModel[]) => {
  return content.map((item) => {
    return {
      ...item,
      sortOrder: 0,
      contentId: item.id,
    } as IFolderContentModel;
  });
};
