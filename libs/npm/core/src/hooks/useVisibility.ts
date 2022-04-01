import throttle from 'lodash.throttle';
import React from 'react';

import { isInViewport } from '../utils/isInViewPort';

/**
 * Check if an element is in viewport

 * @param {number} offset - Number of pixels up to the observable element from the top
 * @param {number} throttleMilliseconds - Throttle observable listener, in ms
 */
export function useVisibility<Element extends HTMLElement>(
  offset: number = 0,
  throttleMilliseconds: number = 100,
): [boolean, React.RefObject<Element>] {
  const [isVisible, setIsVisible] = React.useState(false);
  const ref = React.useRef<Element>(null);

  const onScroll = throttle(() => {
    if (!ref.current) {
      setIsVisible(false);
      return;
    }

    setIsVisible(isInViewport(ref.current));
  }, throttleMilliseconds);

  React.useEffect(() => {
    document.addEventListener('scroll', onScroll, true);
    return () => document.removeEventListener('scroll', onScroll, true);
  });

  return [isVisible, ref];
}
