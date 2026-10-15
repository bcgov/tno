import React from 'react';
import styled from 'styled-components';

const Body = styled.div`
  /* Report templates emit sibling blocks; defer their layout and paint until nearby.
     Keep the HTML mounted so find-in-page, selection and report anchors still work. */
  @media screen {
    > div,
    > div > div,
    > div > ul,
    > p,
    > ul,
    > section,
    article,
    .article {
      content-visibility: auto;
      contain-intrinsic-block-size: auto 300px;
    }
  }
`;

/**
 * Scroll an in-report link (table of contents, back, previous/next) to its anchor.
 * The report scrolls inside a container rather than the window, which iOS does not scroll on a
 * fragment link, and blocks rendered on arrival replace their estimated heights and move the
 * anchor, so keep it aligned until the layout settles or the reader scrolls.
 */
const scrollToAnchor = (event: React.MouseEvent<HTMLDivElement>) => {
  if (event.defaultPrevented || event.button !== 0) return;
  if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
  const link = (event.target as Element).closest('a[href^="#"]');
  const id = decodeURIComponent(link?.getAttribute('href')?.slice(1) ?? '');
  if (!id) return;
  const name = CSS.escape(id);
  const target = event.currentTarget.querySelector(`[id="${name}"], [name="${name}"]`);
  if (!target) return;

  event.preventDefault();
  window.history.pushState(null, '', `#${id}`);
  target.scrollIntoView({ block: 'start' });

  let top = target.getBoundingClientRect().top;
  let stableFrames = 0;
  let frames = 0;
  let frame = 0;
  const stop = () => {
    cancelAnimationFrame(frame);
    window.removeEventListener('wheel', stop, true);
    window.removeEventListener('touchstart', stop, true);
    window.removeEventListener('keydown', stop, true);
  };
  const settle = () => {
    const current = target.getBoundingClientRect().top;
    if (current === top) stableFrames++;
    else {
      stableFrames = 0;
      target.scrollIntoView({ block: 'start' });
      top = target.getBoundingClientRect().top;
    }
    if (stableFrames < 3 && ++frames < 60) frame = requestAnimationFrame(settle);
    else stop();
  };
  window.addEventListener('wheel', stop, true);
  window.addEventListener('touchstart', stop, true);
  window.addEventListener('keydown', stop, true);
  frame = requestAnimationFrame(settle);
};

/** Shared by the editable preview and standalone report instance view. */
export const ReportBody = React.memo(({ html }: { html: string }) => (
  <Body
    className="preview-body"
    onClick={scrollToAnchor}
    dangerouslySetInnerHTML={{ __html: html }}
  />
));
