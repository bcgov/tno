import { TopicTypeName } from 'tno-core';

export interface ITopicOptionItem {
  value: number;
  label: string;
  topicType: TopicTypeName;
  isDisabled: boolean;
}
