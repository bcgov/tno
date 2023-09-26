export interface IDirtyValueOptions {
  simplePropertiesOnly?: boolean;
}

export interface IDirtyValues {
  isDirty: boolean;
  values: object;
}

/**
 * Compares two objects and creates a new object containing all dirty properties.
 * Only works with base properties (i.e. string, number).
 * @param newValues Object with new values.
 * @param initialValues Object with initial values.
 * @returns New object containing only dirty fields with the new values.
 */
export const getDirtyValues = <T extends object>(
  newValues: T,
  initialValues: T,
  options: IDirtyValueOptions = {
    simplePropertiesOnly: true,
  },
): IDirtyValues => {
  const data = { ...newValues };
  const keyValues = Object.keys(data);

  const dirtyKeys = keyValues.filter((keyValue) => {
    const newValue = data[keyValue as keyof T];
    const initValue = initialValues[keyValue as keyof T];
    if (options.simplePropertiesOnly && (typeof newValue === 'object' || Array.isArray(newValue)))
      return false;
    return newValue !== initValue;
  });

  keyValues.forEach((key) => {
    if (!dirtyKeys.includes(key)) delete data[key as keyof T];
  });

  return {
    isDirty: !!dirtyKeys.length,
    values: data,
  };
};
