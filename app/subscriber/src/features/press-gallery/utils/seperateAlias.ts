/**
 * @description - Seperate alias into individual strings
 * @param {string[]} alias - The alias to be seperated, may contain strings that are comma seperated or by space
 * @returns {string[]} - The alias seperated into individual strings, wrapped in double quotes to be used in the Elasticsearch query
 */
export const seperateAlias = (alias: string[]) => {
  return alias
    .map((a) => a.split(/,\s*/)) // Only split on comma followed by any amount of whitespace
    .flat()
    .map((alias) => `"${alias}"`);
};
