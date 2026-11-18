import { escape } from 'lodash';
import { renderToStaticMarkup } from 'react-dom/server';
import { describe, expect, it } from 'vitest';

import { truncateTeaser } from '@/subscriber/components/content-list/utils/truncateTeaser';
import { formatSearch } from '@/subscriber/features/search-page/utils/formatSearch';

describe('truncateTeaser', () => {
  it('counts decoded text instead of HTML tags or entities', () => {
    expect(truncateTeaser('<b>A &amp; B</b>', 5)).toBe('A & B');
    expect(truncateTeaser('<b>A &amp; B</b>', 4)).toBe('A & ...');
  });

  it('removes script contents before making a teaser', () => {
    expect(truncateTeaser('<script>danger()</script><p>News</p>', 20)).toBe('News');
  });

  it('keeps encoded markup as text through teaser search highlighting', () => {
    const teaser = truncateTeaser('&lt;img src=x onerror=alert(1)&gt; News', 200);
    const output = document.createElement('div');
    output.innerHTML = renderToStaticMarkup(
      formatSearch(escape(teaser), {
        search: 'News',
        boldKeywords: true,
        searchUnpublished: false,
        size: 20,
      }),
    );
    expect(output.querySelector('img')).toBeNull();
    expect(output.textContent).toBe('<img src=x onerror=alert(1)> News');
    expect(output.querySelector('b')?.textContent).toBe('News');
  });

  it('returns empty text for incomplete tags rather than leftover markup', () => {
    expect(truncateTeaser('<script', 20)).toBe('');
  });
});
