/**
 * Handy way to pause a thread.
 * @param ms Number of milliseconds to pause.
 * @returns A promise of completion.
 */
export const pause = <T>(ms: number, callback: (value: T) => void) => {
  return new Promise((resolve) => setTimeout(callback ?? resolve, ms));
};
