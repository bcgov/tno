/**
 * Get the value for the specified 'key' in the cookie.
 * @param key The key name.
 * @returns The value if exists.
 */
export const getCookie = (key: string) => {
  var b = document.cookie.match('(^|;)\\s*' + key + '\\s*=\\s*([^;]+)');
  return b ? b.pop() : undefined;
};
