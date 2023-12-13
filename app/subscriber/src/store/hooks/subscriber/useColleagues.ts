import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import {
  IColleagueModel,
  INotificationModel,
  IResponseErrorModel,
  IUserColleagueModel,
  useApiSubscriberColleagues,
} from 'tno-core';

interface IColleagueController {
  getColleagues: () => Promise<IColleagueModel[]>;
  addColleague: (email: string) => Promise<IUserColleagueModel> | Promise<IResponseErrorModel>;
  deleteColleague: (model: IColleagueModel) => Promise<IColleagueModel>;
  sendNotification: (
    notificationId: number,
    to: string,
    contentId?: number,
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
      sendNotification: async (notificationId: number, to: string, contentId?: number) => {
        const response = await dispatch<INotificationModel>('send-Notification', () =>
          api.sendNotification(notificationId, to, contentId),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
