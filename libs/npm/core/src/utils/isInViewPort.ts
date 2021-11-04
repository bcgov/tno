/**
 * Determine if the specified element is fully in the viewport.
 * @param element HTMLElement object.
 * @returns True if the element is fully in the viewport.
 */
export const isInViewport = (element: Element | null) => {
  if (element == null || element === undefined) return false;
  const rect = element.getBoundingClientRect();
  return (
    rect.top >= 0 &&
    rect.left >= 0 &&
    rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
    rect.right <= (window.innerWidth || document.documentElement.clientWidth)
  );
};
