import React from 'react';

export const useIntersection = (element: React.RefObject<HTMLElement>, rootMargin?: string) => {
  const [isVisible, setState] = React.useState(false);

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        setState(entry.isIntersecting);
      },
      { rootMargin },
    );

    element.current && observer.observe(element.current);

    return () => observer.unobserve(element.current as HTMLElement);
  }, [element, rootMargin]);

  return isVisible;
};
