import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { type IAdminState, useAdminStore } from 'store/slices';
import { type IPaged, type ISeriesFilter, type ISeriesModel, useApiAdminSeries } from 'tno-core';

interface ISeriesController {
  findAllSeries: () => Promise<ISeriesModel[]>;
  findSeries: (filter: ISeriesFilter) => Promise<IPaged<ISeriesModel>>;
  getSeries: (id: number) => Promise<ISeriesModel>;
  addSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  updateSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  deleteSeries: (model: ISeriesModel) => Promise<ISeriesModel>;
  mergeSeries: (fromSeriesId: number, intoSeriesId: number) => Promise<ISeriesModel>;
}

export const useSeries = (): [IAdminState, ISeriesController] => {
  const api = useApiAdminSeries();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();

  const controller = React.useMemo(
    () => ({
      findAllSeries: async () => {
        const response = await dispatch<ISeriesModel[]>(
          'find-all-series',
          async () => await api.findAllSeries(),
        );
        store.storeSeries(response.data);
        return response.data;
      },
      findSeries: async (filter: ISeriesFilter) => {
        const response = await dispatch<IPaged<ISeriesModel>>(
          'find-series',
          async () => await api.findSeries(filter),
        );
        return response.data;
      },
      getSeries: async (id: number) => {
        const response = await dispatch<ISeriesModel>(
          'get-series',
          async () => await api.getSeries(id),
        );
        store.storeSeries((series) =>
          series.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        return response.data;
      },
      addSeries: async (model: ISeriesModel) => {
        const response = await dispatch<ISeriesModel>(
          'add-series',
          async () => await api.addSeries(model),
        );
        store.storeSeries((series) => [...series, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateSeries: async (model: ISeriesModel) => {
        const response = await dispatch<ISeriesModel>(
          'update-series',
          async () => await api.updateSeries(model),
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
        const response = await dispatch<ISeriesModel>(
          'delete-series',
          async () => await api.deleteSeries(model),
        );
        store.storeSeries((series) => series.filter((ds) => ds.id !== response.data.id));
        await lookup.getLookups();
        return response.data;
      },
      mergeSeries: async (fromSeriesId: number, intoSeriesId: number) => {
        const response = await dispatch<ISeriesModel>(
          'merge-series',
          async () => await api.mergeSeries(fromSeriesId, intoSeriesId),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [state, controller];
};
