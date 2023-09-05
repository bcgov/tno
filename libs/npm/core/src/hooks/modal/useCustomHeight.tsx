import React from 'react';

/*
 * This hook is useful for retrieving the custom height of the browser for the modal popup
 */
export const useCustomHeight = () => {
  const [customHeight, setCustomHeight] = React.useState(window.innerHeight - 235);
  React.useEffect(() => {
    const handleResize = () => {
      setCustomHeight(window.innerHeight - 235);
    };
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);
  return { customHeight };
};
