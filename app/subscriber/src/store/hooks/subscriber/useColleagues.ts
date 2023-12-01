import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IColleagueModel, useApiSubscriberColleagues } from 'tno-core';

interface IColleagueController {
  getColleagues: () => Promise<IColleagueModel[]>;
  addColleague: (model: IColleagueModel) => Promise<IColleagueModel>;
  deleteColleague: (model: IColleagueModel) => Promise<IColleagueModel>;
}

export const useColleagues = (): [IColleagueController] => {
  const api = useApiSubscriberColleagues();
  const dispatch = useAjaxWrapper();

  const controller = React.useMemo(
    () => ({
      getColleagues: async () => {
        const response = await dispatch<IColleagueModel[]>('get-colleagues', () =>
          api.getColleagues(),
        );
        return response.data;
      },
      addColleague: async (model: IColleagueModel) => {
        const response = await dispatch<IColleagueModel>('add-colleague', () =>
          api.addColleague(model),
        );
        return response.data;
      },
      deleteColleague: async (model: IColleagueModel) => {
        const response = await dispatch<IColleagueModel>('delete-colleague', () =>
          api.deleteColleague(model),
        );
        return response.data;
      },
    }),
    [api, dispatch],
  );

  return [controller];
};
