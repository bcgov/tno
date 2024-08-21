import { ITopicModel, TopicTypeName } from 'tno-core';

import { generateRandom } from './generateRandom';

interface IGenerateTopicOptions {
  _id?: number;
  init?: Partial<ITopicModel>;
}

export const generateTopic = (options: IGenerateTopicOptions) => {
  const entity: ITopicModel = {
    id: options?._id ?? 0,
    name: `Topic ${options?._id ?? 0}`,
    description: '',
    sortOrder: 0,
    isEnabled: true,
    topicType: generateRandom(0, 1) ? TopicTypeName.Proactive : TopicTypeName.Issues,
  };
  return entity;
};
