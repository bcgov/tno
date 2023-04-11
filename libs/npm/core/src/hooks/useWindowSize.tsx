import React from 'react';

/*
 * This hook is useful for determining the window size of the browser when deciding on which React component to render. Often pared with the
 * <Show /> component
 */
export const useWindowSize = () => {
  const [width, setWidth] = React.useState<number | undefined>(undefined);
  React.useEffect(() => {
    const handleResize = () => setWidth(window.innerWidth);
    window.addEventListener('resize', handleResize);
    handleResize();
    return () => window.removeEventListener('resize', handleResize);
  }, []);
  return { width };
};
