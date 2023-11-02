import React from 'react';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';

export const useFilterOptions = () => {
  const [{ sources, settings }] = useLookup();
  const [printIds, setPrintIds] = React.useState<{
    dailyPrintId: number;
    weeklyPrintId: number;
    onlinePrintId: number;
  }>();
  const [mediaIds, setMediaIds] = React.useState<{
    cpWireId: number;
    talkRadioId: number;
    newsRadioId: number;
    televisionId: number;
  }>();

  React.useEffect(() => {
    if (settings.length) {
      if (!settings.find((s) => s.name === 'MediaIds'))
        toast.error(
          'MediaIds not found, please contact support to set these through the editor app.',
        );
      if (!settings.find((s) => s.name === 'PrintIds'))
        toast.error(
          'PrintIds not found, please contact support to set these through the editor app.',
        );

      setMediaIds(JSON.parse(settings.find((s) => s.name === 'MediaIds')?.value ?? '{}'));
      setPrintIds(JSON.parse(settings.find((s) => s.name === 'PrintIds')?.value ?? '{}'));
    }
  }, [settings]);

  const dailyPrint = sources.filter((source) => source.productId === printIds?.dailyPrintId);
  const weeklyPrint = sources.filter((source) => source.productId === printIds?.weeklyPrintId);
  const cpWire = sources.filter((source) => source.productId === mediaIds?.cpWireId);
  const talkRadio = sources.filter((source) => source.productId === mediaIds?.talkRadioId);
  const onlinePrint = sources.filter((source) => source.productId === printIds?.onlinePrintId);
  const newsRadio = sources.filter((source) => source.productId === mediaIds?.newsRadioId);
  const television = sources.filter((source) => source.productId === mediaIds?.televisionId);
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
