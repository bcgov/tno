import React from 'react';

/**
 * Scrolls to the specified element Id on page load.
 * @param id A value that when changed will execute the scroll to id.
 * @param scrollToId The element Id to scroll to.
 * @returns void
 */
export const useScrollTo = (id: number | string | undefined | null, scrollToId: string) => {
  React.useEffect(() => {
    if (scrollToId)
      document.getElementById(scrollToId)?.scrollIntoView({
        behavior: 'smooth',
      });
  }, [id, scrollToId]);
};
