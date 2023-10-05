import React from 'react';
import { ILookupController, useLookup } from 'store/hooks';
import { ILookupState } from 'store/slices';
import { getSortableOptions, getSourceOptions, IOptionItem } from 'tno-core';

export interface ILookupOptionsState extends ILookupState {
  ingestTypeOptions: IOptionItem[];
  productOptions: IOptionItem[];
  sourceOptions: IOptionItem[];
  seriesOptions: IOptionItem[];
  contributorOptions: IOptionItem[];
  userOptions: IOptionItem[];
}

/**
 * Simplify converting data sources into options for dropdowns.
 * @returns An array of data source options for dropdowns.
 */
export const useLookupOptions = (): [ILookupOptionsState, ILookupController] => {
  const [state, controller] = useLookup();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [contributorOptions, setContributorOptions] = React.useState<IOptionItem[]>([]);
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions] = React.useState<IOptionItem[]>([]);
  const [ingestTypeOptions, setIngestTypeOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(state.sources));
  }, [state.sources]);

  React.useEffect(() => {
    setSeriesOptions(getSortableOptions(state.series));
  }, [state.series]);

  React.useEffect(() => {
    setContributorOptions(getSortableOptions(state.contributors));
  }, [state.contributors]);

  React.useEffect(() => {
    setProductOptions(getSortableOptions(state.products));
  }, [state.products]);

  React.useEffect(() => {
    setIngestTypeOptions(getSortableOptions(state.ingestTypes));
  }, [state.ingestTypes]);

  return [
    {
      ...state,
      ingestTypeOptions,
      productOptions,
      sourceOptions,
      seriesOptions,
      contributorOptions,
      userOptions,
    },
    controller,
  ];
};
