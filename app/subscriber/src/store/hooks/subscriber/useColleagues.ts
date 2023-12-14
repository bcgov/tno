import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import {
  INotificationModel,
  IResponseErrorModel,
  IUserColleagueModel,
  useApiSubscriberColleagues,
} from 'tno-core';

interface IColleagueController {
  getColleagues: () => Promise<IUserColleagueModel[]>;
  addColleague: (email: string) => Promise<IUserColleagueModel> | Promise<IResponseErrorModel>;
  deleteColleague: (model: IUserColleagueModel) => Promise<IUserColleagueModel>;
  share: (
    contentId: number,
    colleagueId: number,
    notificationId: number,
  ) => Promise<INotificationModel>;
}

export const useColleagues = (): [IColleagueController] => {
  const api = useApiSubscriberColleagues();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getColleagues: async () => {
        const response = await dispatch<IUserColleagueModel[]>('get-colleagues', () =>
          api.getColleagues(),
        );
        return response.data;
      },
      addColleague: async (email: string) => {
        const response = await dispatch<IUserColleagueModel>('add-colleague', () =>
          api.addColleague(email),
        );
        return response.data;
      },
      deleteColleague: async (model: IUserColleagueModel) => {
        const response = await dispatch<IUserColleagueModel>('delete-colleague', () =>
          api.deleteColleague(model),
        );
        return response.data;
      },
      share: async (contentId: number, colleagueId: number, notificationId: number) => {
        const response = await dispatch<INotificationModel>('send-Notification', () =>
          api.share(contentId, colleagueId, notificationId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
