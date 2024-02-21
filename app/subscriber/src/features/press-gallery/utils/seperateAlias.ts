/**
 * @description - Seperate alias into individual strings
 * @param {string[]} alias - The alias to be seperated, may contain strings that are comma seperated or by space
 * @returns {string[]} - The alias seperated into individual strings
 */
export const seperateAlias = (alias: string[]) => {
  return alias.map((a) => a.split(/,\s*|\s+/)).flat();
};
