import { IFileReferenceModel } from 'tno-core';

import { IGroupByState } from './IGroupByState';
import { IToggleStates } from './IToggleStates';

export interface IContentListContext {
  viewOptions: IToggleStates;
  setViewOptions: (options: IToggleStates) => void;
  groupBy: IGroupByState;
  setGroupBy: (groupBy: IGroupByState) => void;
  activeStream: { id: number; source: string };
  setActiveStream: (stream: { id: number; source: string }) => void;
  activeFileReference: IFileReferenceModel | undefined;
  setActiveFileReference: (fileReference: IFileReferenceModel | undefined) => void;
}
