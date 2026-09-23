import { AxiosResponse } from 'axios';
import React from 'react';
import {
  defaultEnvelope,
  ILifecycleToasts,
  IReportModel,
  IUserReportModel,
  useApi,
} from 'tno-core';

/**
 * Hook to make report subscriber requests to the API.
 * @returns CustomAxios object setup for the API.
 */
export const useApiSubscriberReportSubscribers = (
  options: {
    lifecycleToasts?: ILifecycleToasts;
    selector?: Function;
    envelope?: typeof defaultEnvelope;
    baseURL?: string;
  } = {},
) => {
  const api = useApi(options);

  return React.useRef({
    /**
     * Add or update only the specified subscribers of the report.
     * Subscriptions not included are left untouched, so send only what changed.
     * A subscription is never deleted; send `isSubscribed: false` to unsubscribe.
     */
    updateReportSubscribers: (reportId: number, subscribers: IUserReportModel[]) => {
      return api.put<IUserReportModel[], AxiosResponse<IReportModel>, any>(
        `/subscriber/reports/${reportId}/subscribers`,
        subscribers,
      );
    },
  }).current;
};
