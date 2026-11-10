/**
 * @description - Separate alias into individual strings
 * @param {string[]} alias - The alias to be separated, may contain strings that are comma separated or by space
 * @returns {string[]} - The alias separated into individual strings, wrapped in double quotes to be used in the Elasticsearch query
 */
export const separateAlias = (alias: string[]) => {
  return alias
    .map((a) => a.split(/,\s*/)) // Only split on comma followed by any amount of whitespace
    .flat()
    .map((alias) => `"${alias}"`);
};
