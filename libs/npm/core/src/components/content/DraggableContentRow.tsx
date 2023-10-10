import React from 'react';
import { Draggable } from 'react-beautiful-dnd';

import { ContentRow, IContentRowProps } from '.';

export interface IDraggableContentRowProps extends IContentRowProps {
  index: number;
  children?: React.ReactNode;
}

export const DraggableContentRow: React.FC<IDraggableContentRowProps> = ({
  index,
  children,
  ...rest
}) => {
  if (!rest.row.content) return null;
  return (
    <Draggable
      key={`content-${rest.row.content.id}`}
      draggableId={`content-${rest.row.content.id}`}
      index={index}
    >
      {(draggable) => (
        <div
          key={rest.row.content.id}
          className="drag-row"
          ref={draggable.innerRef}
          {...draggable.dragHandleProps}
          {...draggable.draggableProps}
        >
          {children ?? <ContentRow {...rest} />}
        </div>
      )}
    </Draggable>
  );
};
