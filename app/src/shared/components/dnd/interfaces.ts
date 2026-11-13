import type React from 'react';

/** The type of draggables and droppables that do not specify one. */
export const DEFAULT_TYPE = 'DEFAULT';

/** A position in a droppable list. */
export interface DraggableLocation {
  droppableId: string;
  index: number;
}

export interface DragStart {
  draggableId: string;
  type: string;
  source: DraggableLocation;
  mode: 'FLUID';
}

/** The outcome of a drag. `destination` is null when the item was not dropped on a list. */
export interface DropResult extends DragStart {
  destination: DraggableLocation | null;
  reason: 'DROP' | 'CANCEL';
  combine: null;
}

export interface ResponderProvided {
  /** Screen reader announcements are made by the drag and drop library itself. */
  announce: (message: string) => void;
}

export interface DroppableProvided {
  innerRef: (element: HTMLElement | null) => void;
  droppableProps: { 'data-droppable-id': string };
  /** Kept for compatibility; the dragged item keeps its own space, so nothing is rendered. */
  placeholder: React.ReactNode;
}

export interface DroppableStateSnapshot {
  isDraggingOver: boolean;
}

export interface DraggableProvided {
  innerRef: (element: HTMLElement | null) => void;
  draggableProps: { 'data-draggable-id': string };
  /** Spread on the element that starts a drag. Without it the whole item is the handle. */
  dragHandleProps: {
    'data-drag-handle-id': string;
    role: 'button';
    tabIndex: number;
    'aria-roledescription': string;
    draggable: false;
  };
}

export interface DraggableStateSnapshot {
  isDragging: boolean;
  isDropAnimating: boolean;
}
