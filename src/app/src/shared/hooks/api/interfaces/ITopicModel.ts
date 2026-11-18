import { ISortableModel } from '.';
import { TopicTypeName } from '../constants';

export interface ITopicModel extends ISortableModel<number> {
  topicType: TopicTypeName;
  /** The system "Not Applicable" topic. */
  isSystem?: boolean;
}
