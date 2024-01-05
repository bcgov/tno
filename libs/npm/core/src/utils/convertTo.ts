/**
 * Convert the specified 'value' to the specified 'type', or return the specified 'defaultValue'.
 * @param value The value to convert.
 * @param type The type to convert to.
 * @param defaultValue The default value if there is no conversion map or value.
 * @returns The converter value or default value.
 */
export const convertTo = <T>(value: any, type: string, defaultValue: T) => {
  switch (typeof value) {
    case type:
      return value;
    case 'string':
      const text = value as string;
      switch (type) {
        case 'string':
          return text;
        case 'number':
          return Number(text);
        case 'bigint':
          return BigInt(text);
        case 'boolean':
          return text.toLocaleLowerCase() === 'true';
        default:
          return defaultValue;
      }
    case 'number':
      const number = Number(value);
      switch (type) {
        case 'string':
          return number.toString();
        case 'number':
          return number;
        case 'bigint':
          return BigInt(number);
        case 'boolean':
          return !!number;
        default:
          return defaultValue;
      }
    case 'boolean':
      const bool = value as boolean;
      switch (type) {
        case 'string':
          return bool.toString();
        case 'number':
          return bool ? 1 : 0;
        case 'bigint':
          return bool ? 1 : 0;
        case 'boolean':
          return bool;
        default:
          return defaultValue;
      }
    case 'bigint':
      const big = BigInt(value);
      switch (type) {
        case 'string':
          return big.toString();
        case 'number':
          return big;
        case 'bigint':
          return big;
        case 'boolean':
          return !!big;
        default:
          return defaultValue;
      }
    case 'object':
      switch (type) {
        case 'object':
          // Convert each property.
          // Note that typescript can only return one type when it's a composite, which results in invalid values often.
          const keys = Object.keys(value);
          const result: any = {};
          for (const key in value) {
            if (keys.includes(key))
              result[key] = convertTo(
                (value as any)[key],
                typeof (defaultValue as any)[key],
                (defaultValue as any)[key],
              );
          }
          return result as T;
        default:
          return defaultValue;
      }
    default:
      return defaultValue;
  }
};
