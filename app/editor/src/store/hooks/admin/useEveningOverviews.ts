import {
  IEveningOverviewItem,
  IEveningOverviewSection,
} from 'features/admin/evening-overview/interfaces';
import React from 'react';
import { useAjaxWrapper, useLookup } from 'store/hooks';
import { IAdminState, useAdminStore } from 'store/slices';

import { useApiEveningOverviews } from '../api';

interface IEveningOverviewController {
  findAllOverviewSections: () => Promise<IEveningOverviewSection[]>;
  findItemsBySectionId: (sectionId: number) => Promise<IEveningOverviewItem[]>;
  addOverviewSection: (model: IEveningOverviewSection) => Promise<IEveningOverviewSection>;
  addOverviewSectionItem: (model: IEveningOverviewItem) => Promise<IEveningOverviewItem>;
  updateOverviewSectionItem: (model: IEveningOverviewItem) => Promise<IEveningOverviewItem>;
  updateOverviewSection: (model: IEveningOverviewSection) => Promise<IEveningOverviewSection>;
  deleteOverviewSection: (model: IEveningOverviewSection) => Promise<IEveningOverviewSection>;
  deleteOverviewSectionItem: (model: IEveningOverviewItem) => Promise<IEveningOverviewItem>;
}

export const useEveningOverviews = (): [
  IAdminState & { initialized: boolean },
  IEveningOverviewController,
] => {
  const api = useApiEveningOverviews();
  const dispatch = useAjaxWrapper();
  const [state, store] = useAdminStore();
  const [, lookup] = useLookup();
  const [initialized, setInitialized] = React.useState(false);

  const controller = React.useMemo(
    () => ({
      findAllOverviewSections: async () => {
        const response = await dispatch<IEveningOverviewSection[]>(
          'find-all-evening-overviews',
          () => api.findAllEveningOverviewSections(),
        );
        store.storeEveningOverviewSections(response.data);
        setInitialized(true);
        return response.data;
      },
      findItemsBySectionId: async (sectionId: number) => {
        const response = await dispatch<IEveningOverviewItem[]>('find-items-by-section-id', () =>
          api.findItemsBySectionId(sectionId),
        );
        store.storeEveningOverviewItems(response.data);
        setInitialized(true);
        return response.data;
      },
      addOverviewSection: async (model: IEveningOverviewSection) => {
        const response = await dispatch<IEveningOverviewSection>(
          'add-evening-overview-section',
          () => api.addEveningOverviewSection(model),
        );
        store.storeEveningOverviewSections((reports) => [...reports, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      addOverviewSectionItem: async (model: IEveningOverviewItem) => {
        const response = await dispatch<IEveningOverviewItem>('add-evening-overview-section', () =>
          api.addEveningOverviewSectionItem(model),
        );
        store.storeEveningOverviewItems((reports) => [...reports, response.data]);
        await lookup.getLookups();
        return response.data;
      },
      updateOverviewSectionItem: async (model: IEveningOverviewItem) => {
        const response = await dispatch<IEveningOverviewItem>(
          'update-evening-overview-section-item',
          () => api.updateEveningOverviewSectionItem(model),
        );
        store.storeEveningOverviewItems((reports) =>
          reports.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      updateOverviewSection: async (model: IEveningOverviewSection) => {
        const response = await dispatch<IEveningOverviewSection>('update-overview-section', () =>
          api.updateEveningOverviewSection(model),
        );
        store.storeEveningOverviewSections((reports) =>
          reports.map((ds) => {
            if (ds.id === response.data.id) return response.data;
            return ds;
          }),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteOverviewSection: async (model: IEveningOverviewSection) => {
        const response = await dispatch<IEveningOverviewSection>('delete-report', () =>
          api.deleteEveningOverviewSection(model),
        );
        store.storeEveningOverviewSections((reports) =>
          reports.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
      deleteOverviewSectionItem: async (model: IEveningOverviewItem) => {
        const response = await dispatch<IEveningOverviewItem>('delete-item', () =>
          api.deleteEveningOverviewSectionItem(model),
        );
        store.storeEveningOverviewItems((reports) =>
          reports.filter((ds) => ds.id !== response.data.id),
        );
        await lookup.getLookups();
        return response.data;
      },
    }),
    [api, dispatch, lookup, store],
  );

  return [{ ...state, initialized }, controller];
};
