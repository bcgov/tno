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

// Split tokens, but keep quoted phrases and parentheses intact.
const splitTokenGroupsRegex = /"([^"]+)"|\([^)]+\)|[^\s()"]+/g;
// Find all keywords that should be removed from the search.
const removeKeywordsRegex = /^AND|&&|\+|OR|\||\|\|$/g;
// Split tokens by whitespace or quoted phrases, but keep quoted phrases intact.
const slitTokensRegex = /"([^"]*)"|\S+/g;
// Find any not keywords that identify tokens that should be excluded from the search.
const notRegex = /^-|!|NOT/;

/**
 * Extracts tokens from a given text, removing parentheses and keywords.
 * @param text - The text to extract tokens from.
 * @returns new array of tokens, excluding keywords and removing quotes.
 */
const extractTokens = (text: string) => {
  const value = text.replace(/[()]/g, '');
  const tokens = value.match(slitTokensRegex);
  return (
    tokens
      ?.filter((token) => token.match(removeKeywordsRegex) === null)
      ?.map((token) => token.replaceAll('"', '')) || []
  );
};

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

  // Extract all parentheses and their contents.
  const splitGroups = filter.search.match(splitTokenGroupsRegex);
  const tokenGroups =
    splitGroups?.filter((value) => value.trim().match(removeKeywordsRegex) === null) ?? [];
  const tokens: string[] = [];
  let removeNextToken = false;
  for (let i = 0; i < tokenGroups.length; i++) {
    const token = tokenGroups[i].trim();
    const match = token.match(notRegex);
    if (removeNextToken) {
      removeNextToken = false;
      continue;
    }
    if (match === null) {
      // Add the token to the list of search terms.
      const subTokens = extractTokens(token);
      for (let i2 = 0; i2 < subTokens.length; i2++) {
        const part = subTokens[i2].trim();
        tokens.push(part);
      }
    } else if (match.length !== token.length) {
      // This token is an action and is connected to the next token.
      removeNextToken = true;
    }
  }

  if (tokens.length === 0) return parse(text);

  console.debug(tokens);

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
    } else if (token.includes('?')) {
      // Handle ? queries, replace the '?' with '?' with '.?[^\\s\\.\\,\\!\\?]'
      includePatterns.push(`\\b${token.replaceAll('?', '.?[^\\s\\.\\,\\!\\?]')}\\b`);
    } else if (token.startsWith('~')) {
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
  let boldedText = text.replace(includeRegex, `<b><mark>$&</mark></b>`);

  // Use exclude patterns not bold "-"
  if (excludePatterns.length > 0) {
    let excludeRegex = new RegExp(`(${excludePatterns.join('|')})`, 'gi');
    boldedText = boldedText.replace(excludeRegex, (match) => match);
  }

  return parse(boldedText);
};
