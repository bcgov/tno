import { IOptionItem, OptionItem } from 'components/form';
import { IUserModel } from 'hooks/api-editor';
import React from 'react';
import { ILookupController, useLookup } from 'store/hooks';
import { ILookupState } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';
import { getSortableOptions, getSourceOptions } from 'utils';

export interface ILookupOptionsState extends ILookupState {
  ingestTypeOptions: IOptionItem[];
  productOptions: IOptionItem[];
  sourceOptions: IOptionItem[];
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
  const [productOptions, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [userOptions, setUserOptions] = React.useState<IOptionItem[]>([]);
  const [ingestTypeOptions, setIngestTypeOptions] = React.useState<IOptionItem[]>([]);

  const userId =
    state.users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id ?? 0;

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(state.sources));
  }, [state.sources]);

  React.useEffect(() => {
    setProductOptions(getSortableOptions(state.products));
  }, [state.products]);

  React.useEffect(() => {
    setIngestTypeOptions(getSortableOptions(state.ingestTypes));
  }, [state.ingestTypes]);

  React.useEffect(() => {
    // If for some reason the current user does not exist in the local list, go fetch a new list from the api.
    if (!userId) {
      controller.getUsers(true);
    }
    setUserOptions(state.users.map((u) => new OptionItem(u.displayName, u.id)));
  }, [controller, userId, state.users]);

  return [{ ...state, ingestTypeOptions, productOptions, sourceOptions, userOptions }, controller];
};
