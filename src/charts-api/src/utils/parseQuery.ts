import { Request } from 'express';

/**
 * Parse the query string and return a single integer value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns An integer or undefined.
 */
export function getInt(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return parseInt(`${[...value].shift()}`);
  return parseInt(`${value}`);
}

/**
 * Parse the query string and return an array of integer value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns An integer[] or undefined.
 */
export function getIntArray(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return value.map((v) => parseInt(`${value}`));
  return parseInt(`${value}`);
}

/**
 * Parse the query string and return a single float value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns A float or undefined.
 */
export function getFloat(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return value.map((v) => parseFloat(`${value}`));
  return parseFloat(`${value}`);
}

/**
 * Parse the query string and return an array of float value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns An float[] or undefined.
 */
export function getFloatArray(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return value.map((v) => parseFloat(`${value}`));
  return parseFloat(`${value}`);
}

/**
 * Parse the query string and return a single string value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns An string or undefined.
 */
export function getString(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return `${[...value].shift()}`;
  return `${value}`;
}

/**
 * Parse the query string and return an array of string value for the specified 'name'.
 * @param req HTTP request object.
 * @param name The query parameter name.
 * @returns An string[] or undefined.
 */
export function getStringArray(req: Request, name: string) {
  const value = req.query[name];
  if (value === undefined) return undefined;
  if (Array.isArray(value)) return value.map((v) => `${value}`);
  return `${value}`;
}
