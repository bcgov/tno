import React from 'react';
import { useNavigate } from 'react-router';

import { useChartTemplates } from '@/editor/store/hooks/admin';
import { FlexboxTable, Show } from '@/shared';

import { getChartTemplateUsageColumns } from './constants';
import { type IChartTemplateUsageModel } from './interfaces';

export interface IChartTemplateFormUsageProps {
  /** The chart template. */
  chartTemplateId: number;
}

/**
 * Lists the reports that use a chart template.
 * @returns Component.
 */
export const ChartTemplateFormUsage: React.FC<IChartTemplateFormUsageProps> = ({
  chartTemplateId,
}) => {
  const [, { getChartTemplateUsage }] = useChartTemplates();
  const navigate = useNavigate();

  const [reports, setReports] = React.useState<IChartTemplateUsageModel[]>();

  const columns = React.useMemo(() => getChartTemplateUsageColumns(), []);
  const items = React.useMemo(() => reports ?? [], [reports]);

  React.useEffect(() => {
    getChartTemplateUsage(chartTemplateId)
      .then(setReports)
      .catch(() => {});
    // The controller changes with the admin store, which must not repeat this request.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [chartTemplateId]);

  return (
    <div className="chart-usage">
      <Show visible={!!reports && items.length === 0}>
        <p>No reports use this chart template.</p>
      </Show>
      <Show visible={items.length > 0}>
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          pagingEnabled={false}
          onRowClick={(row) => {
            navigate(`/admin/reports/${row.original.id}`);
          }}
        />
      </Show>
    </div>
  );
};
