import { describe, expect, it } from 'vitest';

import { htmlToPlainText } from './htmlToPlainText';

describe('htmlToPlainText', () => {
  it('extracts formatted text and decodes entities', () => {
    expect(htmlToPlainText('<p>News &amp; <strong>views</strong></p>')).toBe('News & views');
  });

  it.each([
    '<script>alert(1)</script>',
    '<scr<script>ipt>alert(1)</scr</script>ipt>',
    '<img src=x onerror=alert(1)>',
    '<svg onload=alert(1)><script>alert(1)</script></svg>',
  ])('does not retain executable elements from %s', (html) => {
    const text = htmlToPlainText(html);
    const output = document.createElement('div');
    output.textContent = text;
    expect(output.querySelector('script, img, svg')).toBeNull();
    expect(document.querySelector('script, img, svg')).toBeNull();
  });

  it('drops script contents and retains surrounding text', () => {
    expect(htmlToPlainText('before<script>alert(1)</script>after')).toBe('beforeafter');
  });

  it('keeps encoded markup as literal text without parsing it again', () => {
    expect(htmlToPlainText('&lt;img src=x onerror=alert(1)&gt;')).toBe(
      '<img src=x onerror=alert(1)>',
    );
  });

  it('handles absent and empty rich text', () => {
    expect(htmlToPlainText()).toBe('');
    expect(htmlToPlainText(null)).toBe('');
    expect(htmlToPlainText('<p><br></p>')).toBe('');
  });
});
