import React, { DependencyList } from 'react';

import { useDeepCompare } from '.';

/** util function used by other useDeep* hooks */
export function useDeepCompareCallback<T extends (...args: any[]) => any>(
  callback: T,
  dependencies: DependencyList,
) {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  return React.useCallback(callback, useDeepCompare(dependencies));
}
