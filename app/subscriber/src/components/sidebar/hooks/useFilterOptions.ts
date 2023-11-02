import React from 'react';
import { useSources } from 'store/hooks/subscriber/useSources';
import { ISourceModel } from 'tno-core';

export const useFilterOptions = () => {
  const [, api] = useSources();
  const [sources, setSources] = React.useState<ISourceModel[]>([]);

  React.useEffect(() => {
    api.findAllSources().then((res) => setSources(res));
    // only run once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const dailyPrint = sources.filter((source) => source.productId === 1);
  const weeklyPrint = sources.filter((source) => source.productId === 2);
  const cpWire = sources.filter((source) => source.productId === 7);
  const talkRadio = sources.filter((source) => source.productId === 5);
  const onlinePrint = sources.filter((source) => source.productId === 3);
  const newsRadio = sources.filter((source) => source.productId === 4);
  const television = sources.filter((source) => source.productId === 6);
  return {
    dailyPrint,
    weeklyPrint,
    cpWire,
    talkRadio,
    onlinePrint,
    newsRadio,
    television,
    sources,
  };
};
