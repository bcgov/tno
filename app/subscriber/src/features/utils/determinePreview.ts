import { trimWords } from 'features/search-page/utils';
import { ContentTypeName, IContentModel } from 'tno-core';

export const determinePreview = (item: IContentModel) => {
  switch (item.contentType) {
    case ContentTypeName.AudioVideo:
      if (item.isApproved && item.body) return trimWords(item.body, 50);
      return item.summary;
    case ContentTypeName.PrintContent:
      return trimWords(item.body ?? '', 50);
    case ContentTypeName.Image:
      return trimWords(item.summary, 50);
    case ContentTypeName.Story:
      return trimWords(item.body ?? '', 50);
    default:
      return '';
  }
};
