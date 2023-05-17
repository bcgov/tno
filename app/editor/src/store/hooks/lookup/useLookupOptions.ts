import React from 'react';
import { ILookupController, useLookup } from 'store/hooks';
import { ILookupState } from 'store/slices';
import {
  getSortableOptions,
  getSourceOptions,
  IOptionItem,
  IUserModel,
  OptionItem,
  useKeycloakWrapper,
} from 'tno-core';

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
  const keycloak = useKeycloakWrapper();
  const [state, controller] = useLookup();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [contributorOptions, setContributorOptions] = React.useState<IOptionItem[]>([]);
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [ingestTypeOptions, setIngestTypeOptions] = React.useState<IOptionItem[]>([]);

  const userId =
    state.users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id ?? 0;

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

  React.useEffect(() => {
    // If for some reason the current user does not exist in the local list, go fetch a new list from the api.
    if (!userId && !!state.users.length) {
      controller.getUsers(true);
    }
    setUserOptions(state.users.map((u) => new OptionItem(u.displayName, u.id)));
  }, [controller, userId, state.users]);

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
