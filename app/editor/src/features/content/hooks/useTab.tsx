import React from 'react';

let _window: Window | null = null;

export interface ITabOptions {
  useOneTab?: boolean;
}

// TODO: When `useOnTab=true` they get out of sync easily.
export const useTab = (options: ITabOptions = { useOneTab: false }) => {
  const [tab, setTab] = React.useState<Window | null>(_window);

  return React.useCallback(
    (id: number, path: string = '/contents') => {
      if (!tab || tab.closed || !options.useOneTab) {
        _window = window.open(`${path}/${id}`, '_blank');
        setTab(_window);
      } else {
        tab.focus();
      }
    },
    [options.useOneTab, tab],
  );
};
