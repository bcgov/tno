import { IParsedTag, Tag } from '../types';

/**
 * Parse tags with their original format from text
 * @param text The text to parse tags from
 * @returns Array of parsed tags with their original format
 */
export const parseTagsWithOriginalFormat = (text: string): IParsedTag[] => {
  const tagPattern = /\[([^\]]+)\]/g;
  const matches = text.match(tagPattern);
  if (!matches) return [];

  return matches
    .map((match) =>
      match
        .slice(1, -1)
        .split(',')
        .map((tag: string) => tag.trim())
        .map((tag: string) => ({
          tag: tag.toUpperCase(),
          original: tag,
        })),
    )
    .flat();
};

/**
 * Create a map of original tag formats
 * @param parsedTags Array of parsed tags
 * @returns Map of uppercase tag codes to their original format
 */
export const createOriginalFormatMap = (parsedTags: IParsedTag[]): Record<string, string> => {
  const map: Record<string, string> = {};
  parsedTags.forEach(({ tag, original }) => {
    map[tag.toUpperCase()] = original;
  });
  return map;
};

/**
 * Get tag codes by their IDs
 * @param tags Array of all available tags
 * @param tagIds Array of tag IDs to get codes for
 * @returns Array of tag codes
 */
export const getTagCodesByIds = (tags: Tag[], tagIds: number[]): string[] => {
  return tags.filter((tag: Tag) => tagIds.includes(tag.id)).map((tag: Tag) => tag.code);
};

/**
 * Update text with tags
 * @param text The current text content
 * @param tagCodes Array of tag codes to include
 * @param originalFormats Map of original tag formats
 * @returns Updated text with formatted tags
 */
export const formatTextWithTags = (
  text: string,
  tagCodes: string[],
  originalFormats: Record<string, string> = {},
): string => {
  // ensure even empty documents work
  const currentText = text ?? '';
  const hasTrailingNewline = currentText?.endsWith('\n') || false;

  // remove all [...] formatted tags
  let newText = (currentText || '').replace(/\s*\[([^\]]*)\](\s|$)*/g, '').trimEnd();

  // add new tags (if any)
  if (tagCodes.length > 0) {
    // use original format to format each tag (if available)
    const formattedTags = tagCodes
      .filter((code) => code && code.trim() !== '') // avoid empty tags
      .map((code) => originalFormats[code.toUpperCase()] || code);

    // only create tag text if there are valid tags
    if (formattedTags.length > 0) {
      const tagText = `[${formattedTags.join(', ')}]`;

      // handle HTML content and plain text
      if (currentText?.includes('</p>')) {
        newText = newText.replace(/<\/p>\s*$/, '');
        newText = newText ? `${newText} ${tagText}</p>` : `<p>${tagText}</p>`;
      } else {
        newText = newText ? `${newText} ${tagText}` : tagText;
      }
    }
  }

  // preserve original trailing newline
  if (hasTrailingNewline) {
    newText += '\n';
  }

  return newText;
};
