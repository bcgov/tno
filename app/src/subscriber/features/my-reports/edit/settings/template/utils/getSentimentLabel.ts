import { IContentModel } from 'tno-core';

export const getSentimentLabel = (content?: IContentModel): string | undefined => {
  const value = content?.tonePools.length ? content.tonePools[0].value : undefined;

  if (value === undefined) return undefined;
  if (value < 0) return 'Negative';
  if (value > 1) return 'Positive';
  return 'Neutral';
};
