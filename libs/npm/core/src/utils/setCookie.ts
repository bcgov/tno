export interface ICookieOptions {
  path?: string;
  domain?: string;
  sameSite?: 'None' | 'Strict' | 'Lax';
  secure?: boolean;
  httpOnly?: boolean;
  hostOnly?: boolean;
  /** Date in GMT */
  expires?: string;
  /** Age in seconds */
  maxAge?: number;
}

/**
 * Adds the specified 'key' and 'value' to a cookie.
 * @param key The cookie key name
 * @param value The cookie key value
 * @param options The cookie configuration options
 */
export const setCookie = (key: string, value: any, options?: ICookieOptions) => {
  const config = {
    sameSite: 'None',
    secure: true,
    expires: 'Fri, 31 Dec 9999 23:59:59 GMT',
    ...options,
  };
  if (config.path && config.path.split('/').length > 1)
    config.path = `/${config.path.split('/')[1]}`;
  var cookie = `${key}=${encodeURIComponent(value)};`;
  if (config.path) cookie = `${cookie}Path=${config.path};`;
  if (config.domain) cookie = `${cookie}Domain=${config.domain};`;
  if (config.secure) cookie = `${cookie}Secure;`;
  if (config.httpOnly) cookie = `${cookie}HttpOnly;`;
  if (config.hostOnly) cookie = `${cookie}HostOnly;`;
  if (config.sameSite) cookie = `${cookie}SameSite=${config.sameSite};`;
  if (config.expires) cookie = `${cookie}Expires=${config.expires};`;
  if (config.maxAge) cookie = `${cookie}MaxAge=${config.maxAge};`;
  document.cookie = cookie;
  return cookie;
};
