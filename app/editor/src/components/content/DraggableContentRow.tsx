import React from 'react';
import { Draggable, DraggableProps } from 'react-beautiful-dnd';

import { ContentRow, IContentRowProps } from '.';

export interface IDraggableContentRowProps
  extends Omit<DraggableProps, 'children'>,
    IContentRowProps {
  className?: string;
  children?: ((props: IDraggableContentRowProps) => React.ReactNode) | React.ReactNode;
}

export const DraggableContentRow: React.FC<IDraggableContentRowProps> = ({
  draggableId,
  index,
  className,
  children,
  ...rest
}) => {
  if (!rest.row.content) return null;

  return (
    <Draggable draggableId={draggableId} index={index}>
      {(draggable) => (
        <div
          className={`drag-row${className ? ` ${className}` : ''}`}
          ref={draggable.innerRef}
          {...draggable.dragHandleProps}
          {...draggable.draggableProps}
        >
          {typeof children === 'function'
            ? (children as (props: IDraggableContentRowProps) => React.ReactNode)({
                draggableId,
                index,
                className,
                ...rest,
              })
            : children ?? <ContentRow {...rest} />}
        </div>
      )}
    </Draggable>
  );
};
