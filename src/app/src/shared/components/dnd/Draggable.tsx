import { useSortable } from '@dnd-kit/react/sortable';
import React from 'react';

import { DroppableContext } from './DroppableContext';
import { type DraggableProvided, type DraggableStateSnapshot } from './interfaces';

export interface IDraggableProps {
  /** Unique within the DragDropContext; reported as `draggableId` in drop results. */
  draggableId: string;
  /** Position of the item within its Droppable list. */
  index: number;
  /** Prevents this item from being dragged. */
  isDragDisabled?: boolean;
  children: (provided: DraggableProvided, snapshot: DraggableStateSnapshot) => React.ReactNode;
}

/** An item that can be sorted within its Droppable list (or moved to another of the same type). */
export const Draggable: React.FC<IDraggableProps> = ({
  draggableId,
  index,
  isDragDisabled = false,
  children,
}) => {
  const list = React.useContext(DroppableContext);
  const { ref, handleRef, isDragging, isDropping } = useSortable({
    id: draggableId,
    index,
    group: list.droppableId,
    type: list.type,
    accept: list.type,
    disabled: { draggable: isDragDisabled, droppable: list.isDropDisabled },
  });

  const element = React.useRef<HTMLElement | null>(null);
  const innerRef = React.useCallback(
    (value: HTMLElement | null) => {
      element.current = value;
      ref(value);
    },
    [ref],
  );

  // The drag handle is whichever element received dragHandleProps.
  React.useLayoutEffect(() => {
    const item = element.current;
    if (!item) return;
    const selector = `[data-drag-handle-id="${CSS.escape(draggableId)}"]`;
    handleRef(item.matches(selector) ? item : item.querySelector(selector));
  });

  return (
    <>
      {children(
        {
          innerRef,
          draggableProps: { 'data-draggable-id': draggableId },
          dragHandleProps: {
            'data-drag-handle-id': draggableId,
            role: 'button',
            tabIndex: 0,
            'aria-roledescription': 'draggable',
            draggable: false,
          },
        },
        { isDragging, isDropAnimating: isDropping },
      )}
    </>
  );
};
