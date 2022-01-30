import { IKeyValuePair } from './interfaces';

/**
 * Extract the keys as an array from the specified enum object.
 * @param enumObject Enum object.
 * @returns An array of key values.
 */
export const enumKeys = (enumObject: any) => {
  return Object.entries(enumObject)
    .filter(([key]) => typeof enumObject[key as any] === 'string')
    .map(([, value]) => value);
};

/**
 * Extract the values as an array from the specified enum object.
 * @param enumObject Enum object.
 * @returns An array of values.
 */
export const enumValues = (enumObject: any) => {
  return Object.entries(enumObject)
    .filter(([key]) => typeof enumObject[key as any] === 'string')
    .map(([, value]) => value);
};

/**
 * Extract the key value pairs as an array from the specified object.
 * @param enumObject Enum object.
 * @returns An array of IKeyValuePair objects.
 */
export const enumKeyValues = (enumObject: any) => {
  return Object.entries(enumObject)
    .filter(([key]) => typeof enumObject[key as any] === 'string')
    .map(([key, value]) => ({ key: value, value: key } as IKeyValuePair));
};
