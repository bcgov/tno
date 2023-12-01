import { AxiosResponse } from 'axios';
import React from 'react';

import { defaultEnvelope, IColleagueModel, ILifecycleToasts, useApi } from '..';

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
    addColleague: (colleague: IColleagueModel) => {
      return api.post<IColleagueModel, AxiosResponse<IColleagueModel>, any>(
        '/subscriber/users/colleagues',
        colleague,
      );
    },
    deleteColleague: (colleague: IColleagueModel) => {
      return api.delete<IColleagueModel, AxiosResponse<IColleagueModel>, any>(
        `/subscriber/users/colleagues/${colleague.colleagueId}`,
        { data: colleague },
      );
    },
  }).current;
};
