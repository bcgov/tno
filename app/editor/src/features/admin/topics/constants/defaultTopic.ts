import { ITopicModel, TopicTypeName } from 'hooks';

export const defaultTopic: ITopicModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 1,
  topicType: TopicTypeName.Issues,
};
