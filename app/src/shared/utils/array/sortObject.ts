/**
 * Sorts the array of objects by the specified predicate value.
 * @param predicate Function to extract a property from the object.
 * @param desc Whether to sort descending order.
 * @returns -1 if a < b, 1 if a > b, or 0 if equal.
 */
export const sortObject = <T>(
  predicate: (item: T) => number | string | undefined | null,
  desc: boolean = false,
) => {
  return (a: T, b: T) => {
    const av = predicate(a);
    const bv = predicate(b);
    const dir = desc ? -1 : 1;
    if (av === bv) return 0;
    else if (av === undefined && bv === undefined) return 0;
    else if (av === null && bv === null) return 0;
    else if (av === undefined || av === null) return -1 * dir;
    else if (bv === undefined || bv === null) return 1 * dir;
    else if (av < bv) return -1 * dir;
    else if (av > bv) return 1 * dir;
    return 0;
  };
};
