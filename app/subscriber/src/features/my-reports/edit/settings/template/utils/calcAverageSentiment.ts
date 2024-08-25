import { IContentModel } from 'tno-core';

import { getSentimentValue } from './getSentimentValue';

export const calcAverageSentiment = (content: (IContentModel | undefined)[]) => {
  const items = content
    .filter((c) => c !== undefined)
    .map((c) => getSentimentValue(c!))
    .filter((v) => v !== undefined);
  if (items.length === 0) return undefined;
  return Math.round(
    items.reduce((previousValue, currentValue) => previousValue! + currentValue!, 0)! /
      items.length,
  );
};
