/** The maximum length of a rule page (prefix plus number). */
export const maxPageLength = 5;

/**
 * Parse a page as a letter prefix and a number ("A12" is prefix "A", number 12).
 * @param page The page value.
 * @returns The prefix and number, or undefined when there is no page number.
 */
export const parsePage = (page?: string): { prefix: string; number: number } | undefined => {
  const match = /^\s*([a-zA-Z]*)\s*(\d+)/.exec(page ?? '');
  if (!match) return undefined;
  return { prefix: match[1], number: Number(match[2]) };
};

/**
 * Format a rule's page range for display.
 * @param pageMin The minimum page.
 * @param pageMax The maximum page.
 * @returns The range, or an empty string when the rule sets no page.
 */
export const formatPageRange = (pageMin?: string, pageMax?: string) => {
  if (pageMin && pageMax) return pageMin === pageMax ? pageMin : `${pageMin}–${pageMax}`;
  if (pageMin) return `≥ ${pageMin}`;
  if (pageMax) return `≤ ${pageMax}`;
  return '';
};

/**
 * Format a numeric or time range for display.
 * @param min The minimum.
 * @param max The maximum.
 * @param wraps Whether a minimum after the maximum wraps (times of day).
 * @returns The range, or an empty string when neither is set.
 */
export const formatRange = (
  min?: string | number,
  max?: string | number,
  wraps: boolean = false,
) => {
  const hasMin = min !== undefined && min !== '';
  const hasMax = max !== undefined && max !== '';
  if (hasMin && hasMax) return `${min}–${max}${wraps && min! > max! ? ' (overnight)' : ''}`;
  if (hasMin) return `≥ ${min}`;
  if (hasMax) return `≤ ${max}`;
  return '';
};
