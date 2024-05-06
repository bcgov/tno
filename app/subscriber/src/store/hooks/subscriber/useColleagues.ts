import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useProfileStore } from 'store/slices';
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
  shareEmail: (
    contentId: number,
    email: string,
    notificationId: number,
  ) => Promise<INotificationModel>;
}

export const useColleagues = (): [IColleagueController] => {
  const api = useApiSubscriberColleagues();
  const dispatch = useAjaxWrapper();
  const [, { storeMyColleagues }] = useProfileStore();

  const controller = React.useMemo(
    () => ({
      getColleagues: async () => {
        const response = await dispatch<IUserColleagueModel[]>('get-colleagues', () =>
          api.getColleagues(),
        );
        storeMyColleagues(response.data);
        return response.data;
      },
      addColleague: async (email: string) => {
        const response = await dispatch<IUserColleagueModel>('add-colleague', () =>
          api.addColleague(email),
        );
        storeMyColleagues((colleagues) =>
          [response.data, ...colleagues].sort((a, b) => {
            const aName = a.colleague?.displayName ?? '';
            const bName = a.colleague?.displayName ?? '';
            return aName < bName ? -1 : aName > bName ? 1 : 0;
          }),
        );
        return response.data;
      },
      deleteColleague: async (model: IUserColleagueModel) => {
        const response = await dispatch<IUserColleagueModel>('delete-colleague', () =>
          api.deleteColleague(model),
        );
        storeMyColleagues((colleagues) =>
          colleagues.filter((c) => c.colleagueId !== model.colleagueId),
        );
        return response.data;
      },
      share: async (contentId: number, colleagueId: number, notificationId: number) => {
        const response = await dispatch<INotificationModel>('send-Notification', () =>
          api.share(contentId, colleagueId, notificationId),
        );
        return response.data;
      },
      shareEmail: async (contentId: number, email: string, notificationId: number) => {
        const response = await dispatch<INotificationModel>('send-Notification-Email', () =>
          api.shareEmail(contentId, email, notificationId),
        );
        return response.data;
      },
    }),
    [api, dispatch, storeMyColleagues],
  );

  return [controller];
};
