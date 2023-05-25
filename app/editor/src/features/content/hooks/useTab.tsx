import React from 'react';
import { useChannel } from 'store/hooks';

export const useTab = () => {
  const channel = useChannel<any>({});

  const [tab, setTab] = React.useState<Window | null>(null);

  return React.useCallback(
    (id: number, path: string = '/contents') => {
      if (!tab || tab.closed) setTab(window.open(`${path}/${id}`, '_blank'));
      else {
        channel('fetch', id);
        tab.focus();
      }
    },
    [channel, tab],
  );
};
