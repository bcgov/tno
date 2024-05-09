import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import { IGroupOption } from 'features/search-page/components/advanced-search/interfaces/IGroupOption';
import { useEffect, useMemo, useState } from 'react';
import { useApp } from 'store/hooks';
import { IMediaTypeModel, ISeriesModel, ISourceModel, ListOptionName } from 'tno-core';

interface ILookup {
  id: number;
  label: string;
  sortOrder: number;
  options: IGroupOption[];
}

const getSourceName = (source: ISourceModel) => {
  const name = source.shortName ? source.shortName : source.name;
  const code =
    name.toLowerCase() === source.code.toLowerCase() ? '' : ` [${source.code.toUpperCase()}]`;
  return `${name}${code}`;
};

/**
 * Custom hook to create and manage sub media groups.
 * @param sources Array of source models.
 * @param mediaTypes Array of media type models.
 * @returns An object containing subMediaGroups, mediaGroupExpanded, and setMediaGroupExpanded function.
 */
export const useSubMediaGroups = (
  sources: ISourceModel[],
  series: ISeriesModel[],
  mediaTypes: IMediaTypeModel[],
): {
  subMediaGroups: ISubMediaGroupItem[];
} => {
  const [subMediaGroups, setSubMediaGroups] = useState<ISubMediaGroupItem[]>([]);
  const [{ userInfo }] = useApp();

  // Determine mediaTypeSourceLookup only when sources or mediaTypes change
  const mediaTypeSourceLookup = useMemo(() => {
    const lookup: ILookup[] = [];
    mediaTypes
      .filter((x) => x.listOption === ListOptionName.Source)
      .forEach((mt) => {
        const l: ILookup = {
          id: mt.id,
          label: mt.name,
          sortOrder: mt.sortOrder,
          options: [],
        };
        sources
          .filter(
            (s) =>
              s.mediaTypeSearchMappings !== undefined &&
              s.mediaTypeSearchMappings.length > 0 &&
              s.mediaTypeSearchMappings.findIndex((x) => x.id === mt.id) > -1,
          )
          .forEach((s) => {
            l.options.push({
              id: s.id,
              listOption: ListOptionName.Source,
              name: getSourceName(s),
              sortOrder: s.sortOrder,
              selected: false,
            });
          });
        lookup.push(l);
      });
    return lookup;
  }, [mediaTypes, sources]);

  // Determine mediaTypeSourceLookup only when sources or mediaTypes change
  const mediaTypeSeriesLookup = useMemo(() => {
    const lookup: ILookup[] = [];
    mediaTypes
      .filter((x) => x.listOption === ListOptionName.Series)
      .forEach((mt) => {
        const l: ILookup = {
          id: mt.id,
          label: mt.name,
          sortOrder: mt.sortOrder,
          options: [],
        };
        series
          .filter(
            (s) =>
              s.mediaTypeSearchMappings !== undefined &&
              s.mediaTypeSearchMappings.length > 0 &&
              s.mediaTypeSearchMappings.findIndex((x) => x.id === mt.id) > -1,
          )
          .forEach((s) => {
            l.options.push({
              id: s.id,
              listOption: ListOptionName.Series,
              name: s.name,
              sortOrder: s.sortOrder,
              selected: false,
            });
          });
        lookup.push(l);
      });
    return lookup;
  }, [series, mediaTypes]);

  useEffect(() => {
    const subGroups: ISubMediaGroupItem[] = [];
    mediaTypeSourceLookup.forEach((x) => {
      subGroups.push({
        key: x.id,
        label: x.label,
        sortOrder: x.sortOrder,
        options: x.options,
        listOption: ListOptionName.Source,
      });
    });
    mediaTypeSeriesLookup.forEach((x) => {
      subGroups.push({
        key: x.id,
        label: x.label,
        sortOrder: x.sortOrder,
        options: x.options,
        listOption: ListOptionName.Series,
      });
    });

    setSubMediaGroups(
      subGroups
        .sort((a, b) => a.sortOrder - b.sortOrder)
        .filter((sg) => !userInfo?.mediaTypes.includes(sg.key)),
    );
  }, [
    mediaTypeSeriesLookup,
    mediaTypeSourceLookup,
    mediaTypes,
    series,
    sources,
    userInfo?.mediaTypes,
  ]);

  return {
    subMediaGroups,
  };
};
