import { IParsedTag, Tag } from '../types';

/**
 * Parse tags with their original format from text
 * @param text The text to parse tags from
 * @returns Array of parsed tags with their original format,
 * input [tag1, tag2], output [{tag: 'TAG1', original: 'tag1'}, {tag: 'TAG2', original: 'tag2'}]
 */
export const parseTagsWithOriginalFormat = (text: string): IParsedTag[] => {
  // Only match tags at the very end of the text after any content
  // This regex looks for tags that appear before </p> or at the end of text
  const tagPattern = /(\s*\[([^\]]+)\])+(?=<\/p>|$)/;
  const match = text.match(tagPattern);
  if (!match) return [];

  // Extract all tag blocks from the matched portion
  const tagBlocks = match[0].match(/\[([^\]]+)\]/g);
  if (!tagBlocks) return [];

  // Parse all tags from all blocks at the end
  const allTags: IParsedTag[] = [];
  tagBlocks.forEach((block) => {
    const tagContent = block.slice(1, -1); // Remove [ and ]
    tagContent.split(',').forEach((tag) => {
      const trimmedTag = tag.trim();
      if (trimmedTag) {
        allTags.push({
          tag: trimmedTag.toUpperCase(),
          original: trimmedTag,
        });
      }
    });
  });

  return allTags;
};

/**
 * Create a map of original tag formats
 * @param parsedTags Array of parsed tags
 * @returns Map of uppercase tag codes to their original format,
 * input [{tag: 'TAG1', original: 'tag1'}, {tag: 'TAG2', original: 'tag2'}], output {'TAG1': 'tag1', 'TAG2': 'tag2'}
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
 * input  alltags: [{id: 1, code: 'tag1'}, {id: 2, code: 'tag2'}, {id: 3, code: 'tag3'}], tagIds: [1, 2], output ['tag1', 'tag2']
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

  // Only remove tags at the end of the text (after content), preserve tags in middle lines
  // This regex removes all consecutive tags that appear before </p> or at the end of text
  let newText = (currentText || '').replace(/(\s*\[([^\]]*)\])+(?=<\/p>|$)/, '').trimEnd();

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
