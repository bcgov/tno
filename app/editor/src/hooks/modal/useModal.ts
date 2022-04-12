import React from 'react';

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
