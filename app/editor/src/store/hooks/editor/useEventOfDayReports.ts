import { AxiosResponse } from 'axios';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { IFolderContentModel, IReportResultModel, useApi } from 'tno-core';

export interface IEventOfDayInstance {
  id: number;
  publishedOn: string;
  sentOn?: string;
  subject: string;
  version: number;
  /** Stories linked to the instance across all of its sections. */
  contentIds: number[];
}

export interface IEventOfDayPreviewRequest {
  /** Stories whose topics were edited; the preview waits until they are re-indexed. */
  changedContentIds: number[];
  /** Stories to add to the folder sections; each must have a topic and be published that day. */
  addContentIds: number[];
  /** Rebuild the folder sections from the folder's collector filter instead of the saved links. */
  recollect: boolean;
}

export interface IEventOfDayHistory {
  instances: IEventOfDayInstance[];
  content: IFolderContentModel[];
}

export interface IEventOfDayPreview extends IReportResultModel {
  instanceId: number;
  version: number;
  publishedOn?: string;
  content: { contentId: number; sectionName: string; sortOrder: number }[];
}

export const useEventOfDayReports = () => {
  const api = useApi();
  const dispatch = useAjaxWrapper();
  const endpoints = React.useRef({
    find: (date: string) =>
      api.get<never, AxiosResponse<IEventOfDayHistory>, any>(
        `/editor/reports/event-of-the-day?date=${encodeURIComponent(date)}`,
      ),
    // Without a saved instance the date identifies the report to generate; nothing is stored until it is saved.
    preview: (id: number, request: IEventOfDayPreviewRequest, date: string) =>
      id
        ? api.post<IEventOfDayPreviewRequest, AxiosResponse<IEventOfDayPreview>, any>(
            `/editor/reports/event-of-the-day/${id}/preview`,
            request,
          )
        : api.post<IEventOfDayPreviewRequest, AxiosResponse<IEventOfDayPreview>, any>(
            `/editor/reports/event-of-the-day/preview?date=${encodeURIComponent(date)}`,
            request,
          ),
    addToFolder: (contentId: number) =>
      api.post<never, AxiosResponse<void>, any>(
        `/editor/reports/event-of-the-day/folder/${contentId}`,
      ),
    save: (preview: IEventOfDayPreview, date: string) =>
      preview.instanceId
        ? api.put<IEventOfDayPreview, AxiosResponse<IEventOfDayInstance>, any>(
            `/editor/reports/event-of-the-day/${preview.instanceId}/output`,
            preview,
          )
        : api.post<IEventOfDayPreview, AxiosResponse<IEventOfDayInstance>, any>(
            `/editor/reports/event-of-the-day/output?date=${encodeURIComponent(date)}`,
            preview,
          ),
  }).current;

  return React.useMemo(
    () => ({
      find: async (date: string) =>
        (await dispatch('find-event-of-day-history', () => endpoints.find(date))).data,
      preview: async (id: number, request: IEventOfDayPreviewRequest, date: string) =>
        (await dispatch('preview-event-of-day-history', () => endpoints.preview(id, request, date)))
          .data,
      addToFolder: async (contentId: number) => {
        await dispatch('add-event-of-day-folder', () => endpoints.addToFolder(contentId));
      },
      save: async (preview: IEventOfDayPreview, date: string) =>
        (await dispatch('save-event-of-day-history', () => endpoints.save(preview, date))).data,
    }),
    [dispatch, endpoints],
  );
};
