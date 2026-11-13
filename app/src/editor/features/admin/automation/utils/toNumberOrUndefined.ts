export const toNumberOrUndefined = (newValue: { value?: unknown } | null): number | undefined => {
  const raw = newValue?.value;
  if (raw === undefined || raw === null || raw === '') return undefined;
  const parsed = Number(raw);
  return Number.isNaN(parsed) ? undefined : parsed;
};
