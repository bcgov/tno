import React from 'react';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { ISourceModel, Settings } from 'tno-core';

export interface IMediaTypeGroupIds {
  cpWireId: number;
  talkRadioId: number;
  newsRadioId: number;
  televisionId: number;
  dailyPrintId: number;
  weeklyPrintId: number;
  onlinePrintId: number;
}

export interface IMediaTypeGroups {
  dailyPrint: ISourceModel[];
  weeklyPrint: ISourceModel[];
  cpWire: ISourceModel[];
  talkRadio: ISourceModel[];
  onlinePrint: ISourceModel[];
  newsRadio: ISourceModel[];
  television: ISourceModel[];
  sources: ISourceModel[];
}

const defaultMediaTypeGroups: IMediaTypeGroups = {
  dailyPrint: [],
  weeklyPrint: [],
  cpWire: [],
  talkRadio: [],
  onlinePrint: [],
  newsRadio: [],
  television: [],
  sources: [],
};

export const useFilterOptions = (): IMediaTypeGroups => {
  const [{ isReady, sources, settings }] = useLookup();
  const [mediaTypeGroups, setMediaTypeGroups] = React.useState<IMediaTypeGroups>({
    ...defaultMediaTypeGroups,
    sources,
  });

  React.useEffect(() => {
    if (isReady) {
      var advancedSearchMediaTypeGroups = settings.find(
        (s) => s.name === Settings.AdvancedSearchMediaTypeGroups,
      )?.value;
      if (advancedSearchMediaTypeGroups) {
        try {
          const values = JSON.parse(advancedSearchMediaTypeGroups);
          setMediaTypeGroups({
            dailyPrint: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.dailyPrintId,
            ),
            weeklyPrint: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.weeklyPrintId,
            ),
            cpWire: sources.filter((source) => source.mediaTypeSearchGroupId === values.cpWireId),
            talkRadio: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.talkRadioId,
            ),
            onlinePrint: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.onlinePrintId,
            ),
            newsRadio: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.newsRadioId,
            ),
            television: sources.filter(
              (source) => source.mediaTypeSearchGroupId === values.televisionId,
            ),
            sources,
          });
        } catch {
          toast.error(
            `Configuration '${Settings.AdvancedSearchMediaTypeGroups}' is invalid, please contact support to set these through the editor app.`,
          );
        }
      } else
        toast.error(
          `Configuration '${Settings.AdvancedSearchMediaTypeGroups}' not found, please contact support to set these through the editor app.`,
        );
    }
  }, [isReady, settings, sources]);

  return mediaTypeGroups;
};
