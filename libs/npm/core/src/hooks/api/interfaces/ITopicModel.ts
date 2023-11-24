import { TopicTypeName } from '../constants';
import { ISortableModel } from '.';

export interface ITopicModel extends ISortableModel<number> {
  topicType: TopicTypeName;
}
