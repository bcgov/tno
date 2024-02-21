/**
 * @desc This function takes an array of strings and seperates them by comma or space
 * @param {string[]} alias - An array of strings
 * @returns {string[]} - An array of strings
 */
export const seperateAlias = (alias: string[]) => {
  return alias.map((a) => a.split(/,\s*|\s+/)).flat();
};
