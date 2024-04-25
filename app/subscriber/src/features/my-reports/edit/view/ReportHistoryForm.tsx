import { Action } from 'components/action';
import { getStatus } from 'features/my-reports/utils';
import { formatDate } from 'features/utils';
import React from 'react';
import { FaSquarePollVertical } from 'react-icons/fa6';
import { useApp, useReports } from 'store/hooks';
import { useReportsStore } from 'store/slices';
import { Col, IReportInstanceModel, Loading, ReportStatusName, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportHistoryForm = () => {
  const { values } = useReportEditContext();
  const [{ requests }] = useApp();
  const [, { findInstancesForReportId }] = useReports();
  const [, { storeReportView }] = useReportsStore();

  const [instances, setInstances] = React.useState<IReportInstanceModel[]>([]);

  const isLoading = requests.some((r) => r.group.includes('view-report'));
  const instanceId = values.instances.length ? values.instances[0].id : undefined;

  const fetchInstances = React.useCallback(
    async (reportId: number) => {
      try {
        const instances = await findInstancesForReportId(reportId);
        setInstances(instances.filter((i) => i.status !== ReportStatusName.Pending));
      } catch {}
    },
    [findInstancesForReportId],
  );

  React.useEffect(() => {
    if (values.id) {
      fetchInstances(values.id);
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId]);

  const handleViewReport = React.useCallback(
    (instance: IReportInstanceModel) => {
      storeReportView({ instanceId: instance.id, subject: instance.subject, body: instance.body });
    },
    [storeReportView],
  );

  return (
    <styled.ReportHistoryForm className="report-edit-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Col className="report-history">
        <div className="col-1">Published On</div>
        <div className="col-2">Sent On</div>
        <div className="col-3">Status</div>
        <div className="col-4"></div>
        {instances.map((instance) => {
          return (
            <React.Fragment key={instance.id}>
              <div className="col-1">{formatDate(instance.publishedOn ?? '', true)}</div>
              <div className="col-2">{formatDate(instance.sentOn ?? '', true)}</div>
              <div className="col-3">{getStatus(instance.status)}</div>
              <div className="col-4">
                <Action
                  icon={<FaSquarePollVertical />}
                  title="View"
                  onClick={() => {
                    handleViewReport(instance);
                  }}
                />
              </div>
            </React.Fragment>
          );
        })}
      </Col>
    </styled.ReportHistoryForm>
  );
};
