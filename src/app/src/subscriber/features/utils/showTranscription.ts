import { ContentTypeName, IContentModel } from '@/shared';

export const showTranscription = (content: IContentModel) => {
  if (content.contentType === ContentTypeName.AudioVideo && content.isApproved && !!content.body) {
    return content.body;
  } else {
    return 'Transcript is not yet available, please check back later.';
  }
};
