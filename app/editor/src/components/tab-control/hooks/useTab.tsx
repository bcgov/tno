import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';

import { NavigateOptions } from '../constants';

/** This is a singleton */
let _window: Window | null = null;

/**
 * Interface representing URL parameters.
 */
export interface IUrlParams {
  [key: string]: string | number | boolean | undefined;
}

export interface ITabProps {
  /** Override the default open tab option */
  open?: NavigateOptions;
  /** When opening a new tab should the navbar be visible? */
  showNav?: boolean;
}

/**
 * Creates a URL with query parameters based on the provided path, id, and params.
 * @param path - The base path for the URL.
 * @param id - The ID to append to the path.
 * @param params - An object containing the query parameters.
 * @returns The generated URL with query parameters.
 */
export function createUrl(path: string, id: number, params: IUrlParams) {
  let url = `${path}/${id}`;
  const queryParams = [];

  // Add query parameters to the URL
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined) {
      queryParams.push(`${encodeURIComponent(key)}=${encodeURIComponent(value)}`);
    }
  }

  if (queryParams.length > 0) {
    url += `?${queryParams.join('&')}`;
  }

  return url;
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
      const url = createUrl(path, id, { showNav: values?.showNav });
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
