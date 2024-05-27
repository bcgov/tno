import React from 'react';
import { Droppable } from 'react-beautiful-dnd';
import { sortObject } from 'tno-core';

import { DraggableContentRow, IContentRowModel } from '.';
import * as styled from './styled';

export interface IDroppableContentContainerProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'onChange'> {
  /** Droppable container id. */
  droppableId?: string;
  /** An array of content. */
  data: IContentRowModel[];
  /** Function to generate route for navigating to content. */
  to?: (row: IContentRowModel) => string;
  /** Function to generate unique id for each row. */
  draggableId?: (row: IContentRowModel) => string;
  /** Whether to show the grip column. */
  showGrip?: boolean;
  /** Whether to show the checkbox for selecting rows. */
  showCheckbox?: boolean;
  /** Whether to show the sort order, which allows manual editing. */
  showSortOrder?: boolean;
  /** The default sort to apply to the rows. */
  defaultSort?: (a: IContentRowModel, b: IContentRowModel) => number;
  /** Remove content item event. */
  onRemove?: (content: IContentRowModel) => void;
  /** Row data has been changed. */
  onChange?: (content: IContentRowModel) => void;
  /** Event fires when a row selection changes. */
  onSelected?: (content: IContentRowModel) => void;
}

export const DroppableContentContainer: React.FC<IDroppableContentContainerProps> = ({
  droppableId = 'content-container',
  draggableId = (row) => row.content.id.toString(),
  data,
  className,
  to,
  showGrip = true,
  showCheckbox,
  showSortOrder,
  defaultSort = sortObject((item) => item.sortOrder),
  onRemove,
  onChange,
  onSelected,
  ...rest
}) => {
  return (
    <Droppable droppableId={droppableId}>
      {(droppable) => (
        <styled.DroppableContentContainer
          className={`drop-container${className ? ` ${className}` : ''}`}
          ref={droppable.innerRef}
          {...rest}
          {...droppable.droppableProps}
        >
          {[...data]
            .sort(defaultSort)
            .filter((row) => !!row.content)
            .map((row, index) => {
              return (
                <DraggableContentRow
                  key={`${droppableId}-${row.content.id}-${index}`}
                  draggableId={draggableId(row)}
                  index={index}
                  row={row}
                  to={to?.(row)}
                  onRemove={onRemove}
                  onChange={onChange}
                  onSelected={onSelected}
                  showGrip={showGrip}
                  showSortOrder={showSortOrder}
                  showCheckbox={showCheckbox}
                ></DraggableContentRow>
              );
            })}
        </styled.DroppableContentContainer>
      )}
    </Droppable>
  );
};
