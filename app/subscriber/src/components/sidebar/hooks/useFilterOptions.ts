import { useLookup } from 'store/hooks';

export const useFilterOptions = () => {
  const [{ sources }] = useLookup();
  const dailyPapers = sources.filter(
    (source) =>
      source.code === 'SUN' ||
      source.code === 'PROVINCE' ||
      source.code === 'TC' ||
      source.code === 'GLOBE' ||
      source.code === 'POST',
  );
  return { dailyPapers, sources };
};
