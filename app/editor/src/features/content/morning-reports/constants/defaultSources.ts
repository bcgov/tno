import { ISourceModel } from 'tno-core';

/** Filter and return the default sources. */
export const defaultSources = (sources: ISourceModel[]) =>
  sources?.filter((source) => ['GLOBE', 'POST', 'PROVINCE', 'TC', 'SUN'].includes(source.code));
