import { AxiosResponse } from 'axios';
import React from 'react';

import { IUserColleagueModel } from '../..';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { useApi } from '..';

/**
 * Common hook to make requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberColleagues = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    getColleagues: () => {
      return api.get<never, AxiosResponse<IUserColleagueModel[]>, any>(
        `/subscriber/users/colleagues`,
      );
    },
    addColleague: (email: string) => {
      return api.post<string, AxiosResponse<IUserColleagueModel>, any>(
        `/subscriber/users/colleagues?email=${encodeURIComponent(email)}`,
      );
    },
    deleteColleague: (model: IUserColleagueModel) => {
      return api.delete<IUserColleagueModel, AxiosResponse<IUserColleagueModel>, any>(
        `/subscriber/users/colleagues/${model.colleague?.id}`,
        { data: model },
      );
    },
  }).current;
};
