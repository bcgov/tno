import { IParsedTag, Tag } from '../types';

/**
 * Parse tags with their original format from text
 * @param text The text to parse tags from
 * @returns Array of parsed tags with their original format,
 * input [tag1, tag2], output [{tag: 'TAG1', original: 'tag1'}, {tag: 'TAG2', original: 'tag2'}]
 */
export const parseTagsWithOriginalFormat = (text: string): IParsedTag[] => {
  // Core regex: Only matches tags at the very end of text or immediately before HTML paragraph closing tags
  // Pattern breakdown:
  // - \[([^\]]+)\] : Matches tag format like [TAG] and captures the content inside brackets
  // - (?=\s*(?:<\/p>|$)) : Positive lookahead that ensures the tag is followed by:
  //   - \s* : Zero or more whitespace characters
  //   - <\/p> : HTML paragraph closing tag
  //   - | : OR operator
  //   - $ : End of string
  //
  // Examples:
  // "bb[a] cc[c] time[ICBC,BC]" → matches [ICBC,BC] (at string end)
  // "<p>content[TAG]</p>" → matches [TAG] (before </p>)
  // "bb[a] cc[c] more text" → matches nothing (no tags at end)
  const tagPattern = /\[([^\]]+)\](?=\s*(?:<\/p>$))/g;
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

  // Removal regex: Only removes tags at the very end of text, preserving all middle-line content
  //
  // Pattern breakdown:
  // - (\s*\[([^\]]*)\])+ : Matches one or more consecutive tag blocks with optional whitespace
  //   - \s* : Zero or more whitespace before each tag
  //   - \[([^\]]*)\] : Tag format [content], allows empty tags
  //   - + : One or more occurrences
  // - (?=<\/p>|$) : Positive lookahead ensuring tags are at end positions
  //
  // Examples:
  //  "bb[a] cc[c] time[ICBC,BC]" → "bb[a] cc[c] time" (removes only end tags)
  //  "<p>content[TAG1][TAG2]</p>" → "<p>content</p>" (removes consecutive end tags)
  //  "bb[a] cc[c] more text" → unchanged (no tags at end to remove)
  let newText = (currentText || '').replace(/(\s*\[([^\]]*)\])+(?=<\/p>$)/, '').trimEnd();

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
