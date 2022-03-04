import React, { DependencyList } from 'react';

import { useDeepCompare } from './useDeepCompare';

/** util function used by other useDeep* hooks */
export function useDeepCompareMemo(factory: () => any, dependencies: DependencyList) {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return React.useMemo(factory, useDeepCompare(dependencies));
}
