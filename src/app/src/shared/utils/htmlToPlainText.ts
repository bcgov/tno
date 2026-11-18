import DOMPurify from 'dompurify';

/** Extract plain text from sanitized HTML. Render the result as text, never as HTML. */
export const htmlToPlainText = (html?: string | null): string =>
  DOMPurify.sanitize(html ?? '', { RETURN_DOM_FRAGMENT: true }).textContent ?? '';
