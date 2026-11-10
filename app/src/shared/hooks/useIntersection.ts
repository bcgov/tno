import React from 'react';

export const useIntersection = (element: React.RefObject<HTMLElement>, rootMargin?: string) => {
  const [isVisible, setState] = React.useState(false);

  const refElement = element.current;

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        setState(entry.isIntersecting);
      },
      { rootMargin },
    );

    refElement && observer.observe(refElement);

    return () => observer.unobserve(refElement as HTMLElement);
  }, [refElement, rootMargin]);

  return isVisible;
};
