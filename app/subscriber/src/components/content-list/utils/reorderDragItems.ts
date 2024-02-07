import { IContentModel } from 'tno-core';

/**
 * Reorder the drag items
 * @param items - The items to reorder
 * @param startIndex - The start index
 * @param endIndex - The end index
 * @returns The items in the new order
 */
export const reorderDragItems = (
  items: IContentModel[],
  startIndex: number,
  endIndex: number,
): IContentModel[] => {
  const result = Array.from(items);
  const [removed] = result.splice(startIndex, 1);
  result.splice(endIndex, 0, removed);
  return result;
};
