import React, { DependencyList, EffectCallback } from 'react';

import { useDeepCompare } from '.';

/**
 * `useDeepCompareEffect` will return a memoized version of the callback that
 * only runs if one of the `deps` has changed.
 *
 * Usage note: only use this if `deps` are objects or arrays that contain
 * objects. Otherwise you should just use React.useEffect.
 *
 */
const useDeepCompareEffect = (callback: EffectCallback, dependencies: DependencyList) => {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  React.useEffect(callback, useDeepCompare(dependencies));
};

export default useDeepCompareEffect;
