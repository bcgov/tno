import { AxiosResponse } from 'axios';
import React from 'react';

import { IColleagueModel } from '../..';
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
      return api.get<never, AxiosResponse<IColleagueModel[]>, any>(`/subscriber/users/colleagues`);
    },
    addColleague: (model: IColleagueModel) => {
      return api.post<IColleagueModel, AxiosResponse<IColleagueModel>, any>(
        '/subscriber/users/colleagues',
        model,
      );
    },
    deleteColleague: (model: IColleagueModel) => {
      return api.delete<IColleagueModel, AxiosResponse<IColleagueModel>, any>(
        `/subscriber/users/colleagues/${model.colleague?.id}`,
        { data: model },
      );
    },
  }).current;
};
