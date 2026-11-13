import { CollisionPriority } from '@dnd-kit/abstract';
import { useDroppable } from '@dnd-kit/react';
import React from 'react';

import { DroppableContext } from './DroppableContext';
import { DEFAULT_TYPE, type DroppableProvided, type DroppableStateSnapshot } from './interfaces';

export interface IDroppableProps {
  /** Identifies the list; reported as `droppableId` in drop results. */
  droppableId: string;
  /** Items can only move between lists of the same type. */
  type?: string;
  /** Prevents dropping into (and reordering within) this list. */
  isDropDisabled?: boolean;
  /** Layout hint kept for compatibility; sorting follows the rendered layout. */
  direction?: 'vertical' | 'horizontal';
  children: (provided: DroppableProvided, snapshot: DroppableStateSnapshot) => React.ReactNode;
}

/**
 * A list that Draggable items can be sorted within, or moved into.
 * The container is itself a drop target so that empty lists can receive items.
 */
export const Droppable: React.FC<IDroppableProps> = ({
  droppableId,
  type = DEFAULT_TYPE,
  isDropDisabled = false,
  children,
}) => {
  const { ref, isDropTarget } = useDroppable({
    id: droppableId,
    type,
    accept: type,
    disabled: isDropDisabled,
    collisionPriority: CollisionPriority.Low,
  });
  const context = React.useMemo(
    () => ({ droppableId, type, isDropDisabled }),
    [droppableId, type, isDropDisabled],
  );

  return (
    <DroppableContext.Provider value={context}>
      {children(
        {
          innerRef: ref,
          droppableProps: { 'data-droppable-id': droppableId },
          placeholder: null,
        },
        { isDraggingOver: isDropTarget },
      )}
    </DroppableContext.Provider>
  );
};
