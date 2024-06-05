import parse from 'html-react-parser';
import { IFilterSettingsModel } from 'tno-core';

/**
 * Escapes special characters in a string to be used in a regular expression.
 * @param string - The string to escape.
 * @returns The escaped string.
 */
function escapeRegExp(string: string): string {
  return string.replace(/[.+?^${}()|[\]\\]/g, '\\$&');
}

/**
 * Formats the search text by applying bold formatting to matched keywords and excluding certain words.
 * @param text - The text to be formatted.
 * @param filter - The filter settings to be applied.
 * @returns The formatted text with matched keywords bolded.
 */
export const formatSearch = (text: string, filter: IFilterSettingsModel) => {
  if (!filter || filter.boldKeywords === false || !filter.search || !filter.search.trim()) {
    return parse(text);
  }

  // Remove parentheses to prevent affecting the construction of the regular expression
  const cleanedSearch = filter.search.replace(/[()]/g, '');
  if (!cleanedSearch) {
    return parse(text);
  }

  // Match possible phrases within quotes, prefixed terms with '*', normal words, and exclude words with '-'
  const tokens = cleanedSearch.match(/"[^"]+"|\b\w+\*|\b\w+\b|-\s*\w+/g) || [];

  let includePatterns: string[] = [];
  let excludePatterns: string[] = [];

  tokens.forEach((token) => {
    if (token.startsWith('"') && token.endsWith('"')) {
      // Handle phrases within quotes, remove the quotes
      includePatterns.push(`\\b${escapeRegExp(token.slice(1, -1))}\\b`);
    } else if (token.endsWith('*')) {
      // Handle prefix queries, remove the '*' at the end
      let prefix = escapeRegExp(token.slice(0, -1));
      includePatterns.push(`\\b${prefix}\\w*\\b`);
    } else if (token.startsWith('-')) {
      // Handle exclude words, remove the '-' at the beginning
      excludePatterns.push(`\\b${escapeRegExp(token.slice(1))}\\b`);
    } else {
      // Handle normal keywords
      includePatterns.push(`\\b${escapeRegExp(token)}\\b`);
    }
  });

  if (includePatterns.length === 0) {
    return parse(text);
  }

  // Build the regular expression and bold the matched words
  let includeRegex = new RegExp(`(${includePatterns.join('|')})`, 'gi');
  let boldedText = text.replace(includeRegex, `<b>$&</b>`);

  // Use exclude patterns not bold "-"
  if (excludePatterns.length > 0) {
    let excludeRegex = new RegExp(`(${excludePatterns.join('|')})`, 'gi');
    boldedText = boldedText.replace(excludeRegex, (match) => match);
  }

  return parse(boldedText);
};
