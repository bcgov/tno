/**
 * Remove [ and ] and everything in between from query
 * @param text Text to remove tags from.
 * @returns new text value
 */
export const removeTags = (text: string) => {
  return text.replace(/\[.*?\]/g, '');
};
