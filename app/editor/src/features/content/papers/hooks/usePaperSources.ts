import { useLookupOptions } from 'store/hooks';

/** Filter and return the default sources. */
export const usePaperSources = () => {
  const [{ sources }] = useLookupOptions();

  return sources?.filter((source) => !!source.configuration.isDailyPaper);
};
