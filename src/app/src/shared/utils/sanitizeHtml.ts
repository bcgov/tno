import DOMPurify from 'dompurify';

/**
 * HTML a user wrote, with anything that can run JavaScript removed: script elements, event
 * handler attributes (onclick, onerror, ...), and javascript: URLs. Markup and text are kept, so
 * the HTML still displays as written.
 * @param html The HTML to clean.
 * @returns The HTML without script.
 */
export const sanitizeHtml = (html?: string | null): string =>
  html ? DOMPurify.sanitize(html) : '';
