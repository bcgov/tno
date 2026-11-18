import { describe, expect, it } from 'vitest';

import { htmlToPromptText } from '@/shared/components/filter-ai/utils/htmlToPromptText';
import { sanitizeHtml } from '@/shared/utils/sanitizeHtml';

import { removeFormatting } from './removeFormatting';

describe('sanitizeHtml', () => {
  it('keeps markup and text', () => {
    expect(sanitizeHtml('<p>Hello <b>world</b> &amp; <a href="https://x.ca">link</a></p>')).toBe(
      '<p>Hello <b>world</b> &amp; <a href="https://x.ca">link</a></p>',
    );
  });

  it('removes anything that runs JavaScript', () => {
    expect(sanitizeHtml('<p>a</p><script>alert(1)</script>')).toBe('<p>a</p>');
    expect(sanitizeHtml('<img src="x" onerror="alert(1)">')).toBe('<img src="x">');
    expect(sanitizeHtml('<a href="javascript:alert(1)">x</a>')).toBe('<a>x</a>');
  });

  it('returns an empty string for nothing', () => {
    expect(sanitizeHtml(undefined)).toBe('');
    expect(sanitizeHtml(null)).toBe('');
  });
});

describe('removeFormatting', () => {
  it('keeps paragraphs and line breaks and drops other formatting, as before', () => {
    expect(removeFormatting('<p class="x">One <strong>bold</strong></p><p>Two<br>three</p>')).toBe(
      '<p>One bold</p><p>Two<br>three</p>',
    );
  });

  it('turns HTML written as text into markup so it displays', () => {
    expect(removeFormatting('<p>&lt;b&gt;bold&lt;/b&gt;</p>')).toBe('<p><b>bold</b></p>');
  });

  it('never produces markup that runs JavaScript', () => {
    expect(removeFormatting('<p>&lt;img src=x onerror=alert(1)&gt;</p>')).toBe(
      '<p><img src="x"></p>',
    );
    expect(removeFormatting('<p>&lt;script&gt;alert(1)&lt;/script&gt;after</p>')).toBe(
      '<p>after</p>',
    );
  });
});

describe('htmlToPromptText', () => {
  it('gives one line per paragraph or list item, as before', () => {
    expect(htmlToPromptText('<p>First</p><ul><li>a</li><li>b</li></ul><p>Last<br>line</p>')).toBe(
      'First\n- a\n- b\nLast\nline',
    );
  });

  it('never carries script into the prompt', () => {
    expect(htmlToPromptText('<p>Hi</p><script>alert(1)</script>')).toBe('Hi');
  });
});
