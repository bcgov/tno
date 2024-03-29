import { AxiosResponse } from 'axios';
import React from 'react';

import { IUserColleagueModel } from '../..';
import { defaultEnvelope, ILifecycleToasts } from '../../summon';
import { INotificationModel, useApi } from '..';

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
    share: (contentId: number, colleagueId: number, notificationId: number) => {
      return api.post<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/subscriber/contents/${contentId}/share?colleagueId=${colleagueId}&notificationId=${notificationId}`,
      );
    },
    shareEmail: (contentId: number, email: string, notificationId: number) => {
      return api.post<INotificationModel, AxiosResponse<INotificationModel>, any>(
        `/subscriber/contents/${contentId}/share/email?email=${email}&notificationId=${notificationId}`,
      );
    },
  }).current;
};
