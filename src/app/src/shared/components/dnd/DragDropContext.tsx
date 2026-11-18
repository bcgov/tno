import { DragDropProvider } from '@dnd-kit/react';
import { isSortable } from '@dnd-kit/react/sortable';
import React from 'react';

import {
  DEFAULT_TYPE,
  type DragStart,
  type DropResult,
  type ResponderProvided,
} from './interfaces';
import { sensors } from './sensors';

export interface IDragDropContextProps {
  /** Called when a drag ends, whether it was dropped on a list or not. */
  onDragEnd: (result: DropResult, provided: ResponderProvided) => void;
  /** Called when a drag starts. */
  onDragStart?: (start: DragStart, provided: ResponderProvided) => void;
  children?: React.ReactNode;
}

const provided: ResponderProvided = { announce: () => {} };

interface IOrigin {
  element: Element;
  parent: Node;
  next: Node | null;
}

/**
 * Drag and drop for sortable lists, with the API of react-beautiful-dnd (deprecated) implemented
 * on @dnd-kit/react. Contains Droppable lists of Draggable items; items can move between lists
 * of the same `type`.
 */
export const DragDropContext: React.FC<IDragDropContextProps> = ({
  onDragEnd,
  onDragStart,
  children,
}) => {
  const origin = React.useRef<IOrigin | undefined>(undefined);

  return (
    <DragDropProvider
      sensors={sensors}
      onDragStart={(event) => {
        const source = event.operation.source;
        if (!isSortable(source)) return;
        const element = source.element;
        origin.current =
          element?.parentNode != null
            ? { element, parent: element.parentNode, next: element.nextSibling }
            : undefined;
        onDragStart?.(
          {
            draggableId: String(source.id),
            type: String(source.type ?? DEFAULT_TYPE),
            source: { droppableId: String(source.initialGroup), index: source.initialIndex },
            mode: 'FLUID',
          },
          provided,
        );
      }}
      onDragEnd={(event) => {
        const { source, target } = event.operation;
        if (!isSortable(source)) return;

        // dnd-kit moves the dragged element in the DOM while sorting and leaves it there on a
        // drop, expecting state to follow. Put it back so the DOM matches what React rendered;
        // the handler's state change (if any) then re-renders the list in its new order.
        // A cancelled drag is restored by dnd-kit itself.
        const from = origin.current;
        origin.current = undefined;
        if (!event.canceled && from && from.element.parentNode) {
          from.parent.insertBefore(
            from.element,
            from.next && from.next.parentNode === from.parent ? from.next : null,
          );
        }

        const dropped = !event.canceled && !!target;
        onDragEnd(
          {
            draggableId: String(source.id),
            type: String(source.type ?? DEFAULT_TYPE),
            source: { droppableId: String(source.initialGroup), index: source.initialIndex },
            destination: dropped
              ? { droppableId: String(source.group), index: source.index }
              : null,
            reason: event.canceled ? 'CANCEL' : 'DROP',
            mode: 'FLUID',
            combine: null,
          },
          provided,
        );
      }}
    >
      {children}
    </DragDropProvider>
  );
};
