import { type TopicTypeName } from '@/shared';

export interface ITopicOptionItem {
  value: number;
  label: string;
  topicType: TopicTypeName;
  isDisabled: boolean;
}
