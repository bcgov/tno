import { IContentModel } from 'tno-core';

export const getSentimentValue = (content?: IContentModel): number | undefined => {
  return content?.tonePools.length ? content.tonePools[0].value : undefined;
};
