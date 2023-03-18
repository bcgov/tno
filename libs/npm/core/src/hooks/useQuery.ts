import React from 'react';
import { useLocation } from 'react-router-dom';

/**
 * Hook to return the current query parameters.
 * @returns URLSearchParams with current URL query parameters.
 */
export const useQuery = () => {
  const { search } = useLocation();

  return React.useMemo(() => new URLSearchParams(search), [search]);
};
