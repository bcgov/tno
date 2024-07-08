import { TopicTypeName } from '../constants';
import { ISortPageFilter } from './ISortPageFilter';

export interface ITopicFilter extends ISortPageFilter {
  name?: string;
  description?: string;
  topicType?: TopicTypeName;
}
