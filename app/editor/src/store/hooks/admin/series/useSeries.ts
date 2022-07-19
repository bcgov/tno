import { IPaged, ISeriesFilter, ISeriesModel, useApiAdminSeries } from 'hooks';
import React from 'react';
import { useApiDispatcher } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

interface ISeriesController {
  findAllSeriess: () => Promise<ISeriesModel[]>;
  findSeries: (filter: ISeriesFilter) => Promise<IPaged<ISeriesModel>>;
  getSeries: (id: number) => Promise<ISeriesModel>;
  addSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  updateSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  deleteSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
}

export const useSeries = (): [IAdminState, ISeriesController] => {
  const api = useApiAdminSeries();
  const dispatch = useApiDispatcher();
  const [state, store] = useAdminStore();

  const controller = React.useMemo(
    () => ({
      findAllSeriess: async () => {
        const result = await dispatch<ISeriesModel[]>('find-all-series', () => api.findAllSeries());
        store.storeSeries(result);
        return result;
      },
      findSeries: async (filter: ISeriesFilter) => {
        const result = await dispatch<IPaged<ISeriesModel>>('find-series', () =>
          api.findSeries(filter),
        );
        return result;
      },
      getSeries: async (id: number) => {
        const result = await dispatch<ISeriesModel>('get-series', () => api.getSeries(id));
        store.storeSeries(
          state.series.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      addSeries: async (model: ISeriesModel) => {
        const result = await dispatch<ISeriesModel>('add-series', () => api.addSeries(model));
        store.storeSeries([...state.series, result]);
        return result;
      },
      updateSeries: async (model: ISeriesModel) => {
        const result = await dispatch<ISeriesModel>('update-series', () => api.updateSeries(model));
        store.storeSeries(
          state.series.map((ds) => {
            if (ds.id === result.id) return result;
            return ds;
          }),
        );
        return result;
      },
      deleteSeries: async (model: ISeriesModel) => {
        const result = await dispatch<ISeriesModel>('delete-series', () => api.deleteSeries(model));
        store.storeSeries(state.series.filter((ds) => ds.id !== result.id));
        return result;
      },
    }),
    // The state.mediaTypes will cause it to fire twice!
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [api, dispatch, store],
  );

  return [state, controller];
};
