import React from 'react';
import { useNavigate } from 'react-router-dom';

export const useNavigateAndScroll = () => {
  const navigate = useNavigate();

  return React.useCallback(
    (path: string) => {
      navigate(path);
      const cont = document.getElementsByClassName('contents-container')[0];
      if (cont) {
        cont.scrollTo(0, 0);
      }
    },
    [navigate],
  );
};
