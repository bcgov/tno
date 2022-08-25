import React from 'react';

/**
 * If the URL path contains the path value '/combined/' then this page is a combined view.
 * @returns True if the path is a combined view.
 */
export const useCombinedView = () => {
  const pathname = window.location.pathname ?? '';
  const [combined, setCombined] = React.useState((pathname.match('/combined/')?.length ?? 0) > 0);

  React.useEffect(() => {
    setCombined((pathname.match('/combined/')?.length ?? 0) > 0);
  }, [pathname]);

  return combined;
};
