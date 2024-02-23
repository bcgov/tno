import { ITopicOptionItem } from './ITopicOptionItem';

export interface IGroupedTopicOptions {
  readonly label: string;
  readonly options: readonly ITopicOptionItem[];
}
