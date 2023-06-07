import React from 'react';
import { useChannel } from 'store/hooks';

let _window: Window | null = null;

export const useTab = () => {
  const channel = useChannel<any>({});

  const [tab, setTab] = React.useState<Window | null>(_window);

  return React.useCallback(
    (id: number, path: string = '/contents') => {
      if (!tab || tab.closed) {
        _window = window.open(`${path}/${id}`, '_blank');
        setTab(_window);
      } else {
        channel('fetch', id);
        tab.focus();
      }
    },
    [channel, tab],
  );
};
