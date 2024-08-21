import { IContentTopicModel } from 'tno-core';

import { generateRandom } from './generateRandom';
import { generateTopic } from './generateTopic';

interface IGenerateContentTopicOptions {
  _id?: number;
  init?: Partial<IContentTopicModel>;
}

export const generateContentTopic = (options: IGenerateContentTopicOptions) => {
  const entity: IContentTopicModel = {
    ...generateTopic(options),
    score: generateRandom(0, 100, 0.5),
  };
  return entity;
};
