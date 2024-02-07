import { IContentSearchResult } from 'features/utils/interfaces';

/**
 * Reorder the drag items
 * @param items - The items to reorder
 * @param startIndex - The start index
 * @param endIndex - The end index
 * @returns The items in the new order
 */
export const reorderDragItems = (
  items: IContentSearchResult[],
  startIndex: number,
  endIndex: number,
): IContentSearchResult[] => {
  // shallow copy of the items
  const result = [...items];
  // remove the item from the start index
  const [removed] = result.splice(startIndex, 1);
  // add the removed item to the end index
  result.splice(endIndex, 0, removed);
  return result;
};
