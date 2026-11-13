/**
 * Generates a random number between two numbers.
 * Rounds number to whole numbers.
 * @param min Minimum number
 * @param max Maximum number
 * @param bias Skew results to the power of [0.0, 1.0]
 * @returns Random number
 */
export const generateRandom = (min: number, max: number, bias?: number): number => {
  if (min > max) throw new Error('min must be equal or greater than max.');

  const difference = Math.abs(min - max);
  const random = Math.floor(Math.random() * (difference + 1)) + min;

  if (bias !== undefined) {
    const rnd = generateRandom(min, max);
    const mix = Math.pow(Math.random(), bias);
    const value = Math.floor(rnd * (1 - mix) + bias * mix + 1);
    return value;
  }

  return random;
};
