/**
 * Calculates the number of pages that contain the total number of items.
 * @param pageSize Number of items on a page.
 * @param itemCount Total number of items in data source.
 * @returns Number of pages.
 */
export const calcPages = (pageSize: number, itemCount?: number) => {
  if (pageSize <= 0) return 0;
  if (!itemCount || itemCount < 0) return 1;
  const pages = Math.floor(itemCount / pageSize);
  if (pages < 1) return 1;
  return itemCount % pageSize > 0 ? pages + 1 : pages;
};
