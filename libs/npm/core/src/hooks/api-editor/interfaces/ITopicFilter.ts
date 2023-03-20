import { TopicTypeName } from '../constants';

export interface ITopicFilter {
  name?: string;
  description?: string;
  topicType?: TopicTypeName;
}
