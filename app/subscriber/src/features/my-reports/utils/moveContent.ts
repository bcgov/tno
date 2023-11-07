import { DropResult } from 'react-beautiful-dnd';
import { IReportInstanceContentModel } from 'tno-core';

import { getDestinationIndex } from './getDestinationIndex';
import { sortContent } from './sortContent';

/**
 * Moves the content to the specified drop location within the report.
 * Resorts content within each section.
 * @param drop The drop event information.
 * @param rows An array of content items.
 * @returns A new array of content items.
 */
export const moveContent = (drop: DropResult, rows: IReportInstanceContentModel[]) => {
  if (!drop.destination) {
    return;
  }
  var newItems = [...rows];

  // Extract key values from draggableId.
  const idParts = drop.draggableId.split('__');
  const sourceSectionName = idParts[0];
  const contentId = +idParts[1];
  const originalIndex = +idParts[2];
  const destinationSectionName = drop.destination.droppableId;

  const isDuplicate =
    sourceSectionName !== destinationSectionName &&
    contentId !== 0 &&
    newItems
      .filter((i) => i.sectionName === destinationSectionName)
      .some((i) => i.contentId === contentId);
  if (isDuplicate) {
    // Remove duplicate content in the same section.
    newItems.splice(originalIndex, 1);
  } else {
    // Move row to destination section.
    const [reOrdered] = newItems.splice(originalIndex, 1);
    const destinationIndex = getDestinationIndex(drop, newItems);
    newItems.splice(destinationIndex, 0, reOrdered);
    // Update the section information.
    reOrdered.sectionName = destinationSectionName;
  }
  return sortContent(newItems);
};
