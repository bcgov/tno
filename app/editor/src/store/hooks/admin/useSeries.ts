import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';
import { IPaged, ISeriesFilter, ISeriesModel, useApiAdminSeries } from 'tno-core';

interface ISeriesController {
  findAllSeries: () => Promise<ISeriesModel[]>;
  findSeries: (filter: ISeriesFilter) => Promise<IPaged<ISeriesModel>>;
  getSeries: (id: number) => Promise<ISeriesModel>;
  addSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  updateSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  deleteSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
}

export const useSeries = (): [IAdminState, ISeriesController] => {
  const api = useApiAdminSeries();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllSeries: async () => {
        const response = await dispatch<ISeriesModel[]>('find-all-series', () =>
          api.findAllSeries(),
        );
        store.storeSeries(response.data);
        return response.data;
      },
      findSeries: async (filter: ISeriesFilter) => {
        const response = await dispatch<IPaged<ISeriesModel>>('find-series', () =>
          api.findSeries(filter),
        );
        return response.data;
      },
      getSeries: async (id: number) => {
        const response = await dispatch<ISeriesModel>('get-series', () => api.getSeries(id));
        store.storeSeries((series) =>
          series.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addSeries: async (model: ISeriesModel) => {
        const response = await dispatch<ISeriesModel>('add-series', () => api.addSeries(model));
        store.storeSeries((series) => [...series, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateSeries: async (model: ISeriesModel) => {
        const response = await dispatch<ISeriesModel>('update-series', () =>
          api.updateSeries(model),
        );
        store.storeSeries((series) =>
          series.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteSeries: async (model: ISeriesModel) => {
        const response = await dispatch<ISeriesModel>('delete-series', () =>
          api.deleteSeries(model),
        );
        store.storeSeries((series) => series.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
