import React from 'react';
import { DragDropContext, DropResult, ResponderProvided } from 'react-beautiful-dnd';
import { sortObject } from 'tno-core';

import { DroppableContentContainer, IContentRowModel } from '.';

export interface IDragDropContentContextProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'onChange'> {
  /** Droppable container id. */
  droppableId?: string;
  /** An array of content. */
  data: IContentRowModel[];
  /** Whether to show the grip column. */
  showGrip?: boolean;
  /** Whether to show the checkbox for selecting rows. */
  showCheckbox?: boolean;
  /** Whether to show the sort order, which allows manual editing. */
  showSortOrder?: boolean;
  /** Function to generate route for navigating to content. */
  to?: (row: IContentRowModel) => string;
  /** The default sort to apply to the rows. */
  defaultSort?: (a: IContentRowModel, b: IContentRowModel) => number;
  /** Returns the new content array with changes. */
  onChange?: (content: IContentRowModel[]) => void;
  /** Event fires when a row selection changes. */
  onSelected?: (content: IContentRowModel) => void;
}

export const DragDropContentContext: React.FC<IDragDropContentContextProps> = ({
  droppableId,
  data,
  showGrip,
  showCheckbox,
  showSortOrder,
  to,
  defaultSort = sortObject((item) => item.sortOrder),
  onChange,
  onSelected,
}) => {
  const handleRemove = React.useCallback(
    (row: IContentRowModel) => {
      const rows = [...data]
        .sort(defaultSort)
        .filter((fc) => fc.content.id !== row.content.id)
        .map((fc, index) => ({ ...fc, sortOrder: index }));
      onChange?.(rows);
    },
    [data, defaultSort, onChange],
  );

  /** function that runs after a user drops an item in the list */
  const handleDrop = React.useCallback(
    (result: DropResult, provided: ResponderProvided) => {
      if (!result.destination) {
        return;
      }
      var rows = [...data].sort(defaultSort);
      const [reorderedItem] = rows.splice(result.source.index, 1);
      rows.splice(result.destination.index, 0, reorderedItem);
      const reorderedContent = rows.map((item, index) => ({
        ...item,
        sortOrder: index,
      }));
      onChange?.(reorderedContent);
    },
    [data, defaultSort, onChange],
  );

  const handleChange = React.useCallback(
    (row: IContentRowModel) => {
      const rows = data.map((d) => (d.content.id === row.content.id ? row : d));
      onChange?.(rows);
    },
    [data, onChange],
  );

  return (
    <DragDropContext onDragEnd={handleDrop}>
      <DroppableContentContainer
        droppableId={droppableId}
        data={data}
        showGrip={showGrip}
        showCheckbox={showCheckbox}
        showSortOrder={showSortOrder}
        onRemove={handleRemove}
        onChange={handleChange}
        onSelected={onSelected}
        to={to}
      />
    </DragDropContext>
  );
};
