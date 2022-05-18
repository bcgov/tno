import React from 'react';

/**
 * Hook that provides common properties and functions to control a modal.
 * @returns Hook properties to control a modal.
 */
export const useModal = () => {
  const [isShowing, setIsShowing] = React.useState(false);
  const toggle = () => {
    setIsShowing(!isShowing);
  };

  return {
    isShowing,
    toggle,
  };
};
