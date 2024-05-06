import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { toQueryString } from 'tno-core';

import { NavigateOptions } from '../constants';

/** This is a singleton */
let _window: Window | null = null;

export interface ITabProps {
  /** Override the default open tab option */
  open?: NavigateOptions;
  /** When opening a new tab should the navbar be visible? */
  showNav?: boolean;
}

/**
 * Hook provides a way to consistently open content links.
 * @param props Hook properties.
 * @returns Object containing methods and state.
 */
export const useTab = (props?: ITabProps) => {
  const [{ options }] = useApp();
  const navigate = useNavigate();
  const [values, setValues] = React.useState<ITabProps>({ open: options?.open, ...props });

  React.useEffect(() => {
    setValues({ open: options?.open, ...props });
    // Only update when redux values change.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [options]);

  const navigateTab = React.useCallback(
    (id: number, path: string = '/contents', open?: NavigateOptions) => {
      const urlParams = { showNav: values?.showNav };
      const queryString = toQueryString(urlParams, {
        includeUndefined: false,
        includeEmpty: false,
      });

      const url = `${path}/${id}?${queryString}`;
      const nav = open ?? values?.open ?? NavigateOptions.NewTab;
      if (nav === NavigateOptions.NewTab) {
        window.open(url, '_blank');
      } else if (nav === NavigateOptions.OnPage) {
        navigate(url);
      } else if (!_window || _window.closed) {
        _window = window.open(url, '_blank');
      } else {
        _window.location.replace(url);
        _window.focus();
      }
    },
    [navigate, values],
  );

  return React.useMemo(
    () => ({
      navigate: navigateTab,
      options,
    }),
    [navigateTab, options],
  );
};
