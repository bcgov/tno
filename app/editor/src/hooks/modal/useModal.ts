import React from 'react';

const useModal = () => {
  const [isShowing, setIsShowing] = React.useState(false);
  const toggle = () => {
    setIsShowing(!isShowing);
  };

  return {
    isShowing,
    toggle,
  };
};

export default useModal;
