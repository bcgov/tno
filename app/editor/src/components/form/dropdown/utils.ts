import { enumKeyValues } from 'utils';

import { IOption } from '..';

/**
 * Determine if the specified object is an instance of IOption.
 * This is accomplished by looking for the discriminator.
 * @param object Object to check.
 * @returns
 */
export function instanceOfIOption(object: any): object is IOption {
  return object.discriminator === 'IOption';
}

/**
 * Cast enum object into an array of IOption.
 * @param enumObject Enum object.
 * @returns An array of IOption.
 */
export const castEnumToOptions = (enumObject: any) => {
  return enumKeyValues(enumObject).map(
    (kv) => ({ label: kv.key, value: kv.value, discriminator: 'IOption' } as IOption),
  );
};
