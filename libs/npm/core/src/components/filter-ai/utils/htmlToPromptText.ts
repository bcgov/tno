/**
 * The prompt editor's HTML as plain text, one paragraph or list item per line, for a prompt
 * written in a plain text area. Text without markup is returned as it is.
 * @param html The prompt as HTML.
 * @returns The prompt as plain text.
 */
export const htmlToPromptText = (html?: string): string => {
  if (!html) return '';
  if (!/<[a-z][^>]*>/i.test(html)) return html;
  const doc = new DOMParser().parseFromString(html, 'text/html');
  doc.querySelectorAll('br').forEach((br) => br.replaceWith('\n'));
  doc.querySelectorAll('li').forEach((li) => li.prepend('- '));
  const blocks = Array.from(doc.body.querySelectorAll('p, li, h1, h2, h3, h4, h5, h6'));
  const text = blocks.length
    ? blocks.map((block) => block.textContent?.trim() ?? '').filter((line) => line.length)
    : [doc.body.textContent?.trim() ?? ''];
  return text.join('\n');
};
