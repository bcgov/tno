export const setArrayValue = <T>(values: T[], value: T | undefined | null, index: number): T[] => {
  const size = values.length > index ? values.length : index + 1;
  const newArray = Array<T | undefined | null>(size);
  for (var itemIndex = 0; itemIndex < size; itemIndex++) {
    const newValue =
      index === itemIndex ? value : values.length > itemIndex ? values[itemIndex] : undefined;
    newArray[itemIndex] = newValue;
  }

  return newArray.filter((value) => value !== undefined && value !== null && value !== '') as T[];
};
