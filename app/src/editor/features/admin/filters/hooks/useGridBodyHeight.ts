import React from 'react';

/** Space kept below the pager: the page's bottom padding (2em) and a small gap. */
const BottomGap = 40;

/**
 * The height that lets a Grid's rows fill the rest of the viewable area: from the top of the rows
 * to the bottom of the window, less the pager. It is measured again when the window resizes or the
 * content above the grid changes height, so the page itself does not need to scroll.
 * @param ref The element containing the Grid.
 * @param minHeight The smallest height, for short windows.
 * @returns The height in pixels, once measured.
 */
export const useGridBodyHeight = (ref: React.RefObject<HTMLElement | null>, minHeight = 200) => {
  const [height, setHeight] = React.useState<number>();

  React.useLayoutEffect(() => {
    const element = ref.current;
    if (!element) return;

    const measure = () => {
      const body = element.querySelector<HTMLElement>('.grid-table:nth-child(2)');
      if (!body) return;
      const pager = element.querySelector<HTMLElement>('.grid-pager');
      const top = body.getBoundingClientRect().top;
      const available = window.innerHeight - top - (pager?.offsetHeight ?? 0) - BottomGap;
      setHeight(Math.max(minHeight, Math.floor(available)));
    };

    measure();
    // The page above the grid (settings, messages) and the window both move the rows' top.
    const observer = new ResizeObserver(measure);
    observer.observe(document.body);
    if (element.parentElement) observer.observe(element.parentElement);
    window.addEventListener('resize', measure);
    return () => {
      observer.disconnect();
      window.removeEventListener('resize', measure);
    };
  }, [ref, minHeight]);

  return height;
};
