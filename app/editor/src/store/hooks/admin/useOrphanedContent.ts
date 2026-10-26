import { AxiosResponse } from 'axios';
import React from 'react';
import { useAjaxWrapper } from 'store/hooks';
import { useApi } from 'tno-core';

export type OrphanSide = 'postgres' | 'elasticsearch';
export type OrphanIndex = 'content' | 'published';
export interface IOrphanedContent {
  id: number;
  headline: string;
  otherSource?: string;
  status: string;
  resolution?: string;
}
export interface IOrphanScan {
  id: string;
  side: OrphanSide;
  index: OrphanIndex;
  status: 'Scanning' | 'Confirming' | 'Completed' | 'Cancelled' | 'Failed';
  startPostedOn?: string;
  endPostedOn?: string;
  scanned: number;
  pending: number;
  found: number;
  cancelRequested: boolean;
  createdOn: string;
  updatedOn: string;
  completedOn?: string;
  error?: string;
}
export interface IOrphanScanResults {
  items: IOrphanedContent[];
  nextAfter: number;
  hasMore: boolean;
}

export const useOrphanedContent = () => {
  const api = useApi();
  const dispatch = useAjaxWrapper();
  return React.useMemo(
    () => ({
      latest: async () => {
        const result = await dispatch('orphan-scan-status', () =>
          api.get<never, AxiosResponse<IOrphanScan | null>, any>('/admin/orphaned-content'),
        );
        return result.data || null;
      },
      start: async (
        side: OrphanSide,
        index: OrphanIndex,
        startPostedOn?: string,
        endPostedOn?: string,
      ) => {
        const result = await dispatch('start-orphan-scan', () =>
          api.post<any, AxiosResponse<IOrphanScan>>('/admin/orphaned-content/scans', {
            side,
            index,
            startPostedOn,
            endPostedOn,
          }),
        );
        return result.data;
      },
      cancel: async (id: string) => {
        await dispatch('cancel-orphan-scan', () =>
          api.post(`/admin/orphaned-content/scans/${id}/cancel`),
        );
      },
      results: async (id: string, after: number) => {
        const result = await dispatch('orphan-scan-results', () =>
          api.get<never, AxiosResponse<IOrphanScanResults>, any>(
            `/admin/orphaned-content/scans/${id}/results`,
            { params: { after, limit: 100 } },
          ),
        );
        return result.data;
      },
      resolve: async (
        id: number,
        action: 'delete' | 'restore' | 'index',
        side: OrphanSide,
        index: OrphanIndex,
        scanId: string,
      ) => {
        const path = `/admin/orphaned-content/${id}`;
        const result = await dispatch('resolve-orphaned-content', () =>
          action === 'delete'
            ? api.delete<never, AxiosResponse<{ message: string }>, any>(path, {
                params: { side, index, scanId },
              })
            : api.post<never, AxiosResponse<{ message: string }>, any>(
                `${path}/${action}`,
                undefined,
                { params: { index, scanId } },
              ),
        );
        return result.data;
      },
    }),
    [api, dispatch],
  );
};
