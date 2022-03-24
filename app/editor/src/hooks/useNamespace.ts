import React from 'react';

/**
 * Simple way to wrap field names with a namespace and array index position.
 * @param namespace The namespace to append to all field names.
 * @param defaultIndex The default index if you want all field names to be part of an array.
 * @returns object with functions
 */
export const useNamespace = (namespace: string, defaultIndex?: number) => {
  return React.useRef({
    field: (name: string, index?: number) => {
      const i = index ?? defaultIndex;
      return `${namespace}${i !== undefined ? `.${i}` : ''}.${name}`;
    },
  }).current;
};
