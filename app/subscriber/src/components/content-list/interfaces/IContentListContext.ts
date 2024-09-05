import { IGroupByState } from './IGroupByState';
import { IToggleStates } from './IToggleStates';

export interface IContentListContext {
  viewOptions: IToggleStates;
  setViewOptions: (options: IToggleStates) => void;
  groupBy: IGroupByState;
  setGroupBy: (groupBy: IGroupByState) => void;
}
