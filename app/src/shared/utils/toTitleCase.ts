/**
 * Formats specified 'text' with title case.
 * @param text Text value to update.
 * @returns Returns text with title case formatting.
 */
export const toTitleCase = (text?: string) => {
  if (!text) return text;

  return text.replace(
    /\w\S*/g,
    (txt) => `${txt.charAt(0).toUpperCase()}${txt.substr(1).toLowerCase()}`,
  );
};
