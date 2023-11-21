import { DropResult } from 'react-beautiful-dnd';
import { IReportInstanceContentModel } from 'tno-core';

/**
 * Determines the destination index in the array the item will be dropped into.
 * This must be called after the source item has been removed from the array.
 * @param drop The drop event information.
 * @param items An array of content.
 * @returns The destination index to drop the item.
 */
export const getDestinationIndex = (drop: DropResult, items: IReportInstanceContentModel[]) => {
  if (!drop.destination) return 0;
  // Find first item from destination section.
  const sectionStartIndex = items.findIndex((i) => i.sectionName === drop.destination?.droppableId);
  return sectionStartIndex + drop.destination.index;
};
