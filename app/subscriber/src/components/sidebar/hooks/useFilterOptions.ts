import React from 'react';
import { useLookup } from 'store/hooks';

export const useFilterOptions = () => {
  const [{ sources }] = useLookup();
  const [constants, setConstants] = React.useState<any>({});

  React.useEffect(() => {
    fetch('/constants.json')
      .then((res) => res.json())
      .then((data) => {
        setConstants(data);
      });
    // only run once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {}, []);

  const dailyPrint = sources.filter((source) => source.productId === constants?.dailyPrintId);
  const weeklyPrint = sources.filter((source) => source.productId === constants?.weeklyPrintId);
  const cpWire = sources.filter((source) => source.productId === constants?.cpWireId);
  const talkRadio = sources.filter((source) => source.productId === constants?.talkRadioId);
  const onlinePrint = sources.filter((source) => source.productId === constants?.onlinePrintId);
  const newsRadio = sources.filter((source) => source.productId === constants?.newsRadioId);
  const television = sources.filter((source) => source.productId === constants?.televisionId);
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
