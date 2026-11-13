import { IContentModel } from '@/shared';

export const getSentimentValue = (content?: IContentModel): number | undefined => {
  return content?.tonePools.length ? content.tonePools[0].value : undefined;
};
