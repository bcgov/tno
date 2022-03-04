/**
 * Compares two values to determine if they have changed.
 * @param first First variable to compare.
 * @param second Second variable to compare.
 * @returns True if the second value is different than the first.
 */
export function hasChanged(first: any, second: any) {
  if (first === second) return false;
  if (typeof first === 'function') return false;
  if (first === undefined && second !== undefined) return true;
  if (first === null && second !== null) return true;
  if (typeof first === 'object') return JSON.stringify(first) === JSON.stringify(second);

  return true;
}
