import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IResponseErrorModel, IUserColleagueModel, useApiSubscriberColleagues } from 'tno-core';

interface IColleagueController {
  getColleagues: () => Promise<IUserColleagueModel[]>;
  addColleague: (email: string) => Promise<IUserColleagueModel> | Promise<IResponseErrorModel>;
  deleteColleague: (model: IUserColleagueModel) => Promise<IUserColleagueModel>;
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
    }),
    [api, dispatch],
  );

  return [controller];
};
