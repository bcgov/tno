import React from 'react';
import { ILookupController, useLookup } from 'store/hooks';
import { ILookupState } from 'store/slices';
import {
  getSortableOptions,
  getSourceOptions,
  IOptionItem,
  ISourceModel,
  IUserModel,
  OptionItem,
  useKeycloakWrapper,
} from 'tno-core';

export interface ILookupOptionsProps {
  sourceMap?: (item: ISourceModel) => IOptionItem<number>;
}

export interface ILookupOptionsState extends ILookupState {
  ingestTypeOptions: IOptionItem[];
  mediaTypeOptions: IOptionItem[];
  sourceOptions: IOptionItem[];
  seriesOptions: IOptionItem[];
  contributorOptions: IOptionItem[];
  userOptions: IOptionItem[];
}

/**
 * Simplify converting data sources into options for dropdowns.
 * @returns An array of data source options for dropdowns.
 */
export const useLookupOptions = ({ sourceMap }: ILookupOptionsProps = {}): [
  ILookupOptionsState,
  ILookupController,
] => {
  const keycloak = useKeycloakWrapper();
  const [state, controller] = useLookup();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [contributorOptions, setContributorOptions] = React.useState<IOptionItem[]>([]);
  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [ingestTypeOptions, setIngestTypeOptions] = React.useState<IOptionItem[]>([]);

  const userId =
    state.users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id ?? 0;

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(state.sources, [], sourceMap));
  }, [sourceMap, state.sources]);

  React.useEffect(() => {
    setSeriesOptions(getSortableOptions(state.series));
  }, [state.series]);

  React.useEffect(() => {
    setContributorOptions(getSortableOptions(state.contributors));
  }, [state.contributors]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(state.mediaTypes));
  }, [state.mediaTypes]);

  React.useEffect(() => {
    setIngestTypeOptions(getSortableOptions(state.ingestTypes));
  }, [state.ingestTypes]);

  React.useEffect(() => {
    // If for some reason the current user does not exist in the local list, go fetch a new list from the api.
    if (!userId && !!state.users.length && state.isReady) {
      controller.getUsers(true).catch();
    }
    setUserOptions(state.users.map((u) => new OptionItem(u.displayName, u.id)));
  }, [controller, userId, state.users, state.isReady]);

  return [
    {
      ...state,
      ingestTypeOptions,
      mediaTypeOptions: mediaTypeOptions,
      sourceOptions,
      seriesOptions,
      contributorOptions,
      userOptions,
    },
    controller,
  ];
};
