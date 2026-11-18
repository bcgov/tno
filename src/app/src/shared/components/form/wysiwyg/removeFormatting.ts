import { sanitizeHtml } from '@/shared/utils/sanitizeHtml';

/**
 * The editor's HTML reduced to its text, keeping only paragraphs and line breaks.
 * Decoding the text turns HTML the user wrote as text (`&lt;b&gt;`) into markup, which is kept so
 * it displays; anything that could run JavaScript is removed, before and after decoding.
 * @param html The editor's HTML.
 * @returns Paragraphs and line breaks around the text, without script.
 */
export const removeFormatting = (html: string): string => {
  const doc = new DOMParser().parseFromString(
    sanitizeHtml(
      html
        .replace(/<p\s*[^>]*>/g, '[p]')
        .replaceAll('</p>', '[/p]')
        .replace(/<br\s*\/?>/g, '[br]'),
    ),
    'text/html',
  );
  return sanitizeHtml(
    doc.body.textContent
      ?.replaceAll('[p]', '<p>')
      .replaceAll('[/p]', '</p>')
      .replaceAll('[br]', '<br>'),
  );
};
