import React from 'react';

import { DEFAULT_TYPE } from './interfaces';

export interface IDroppableContext {
  droppableId: string;
  type: string;
  isDropDisabled: boolean;
}

/** The list a Draggable belongs to. */
export const DroppableContext = React.createContext<IDroppableContext>({
  droppableId: '',
  type: DEFAULT_TYPE,
  isDropDisabled: false,
});
