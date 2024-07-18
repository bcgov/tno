import { Action } from 'components/action';
import { getStatus } from 'features/my-reports/utils';
import { formatDate, formatTime } from 'features/utils';
import React from 'react';
import { FaClockRotateLeft, FaEye, FaRegClock } from 'react-icons/fa6';
import { useApp, useReports } from 'store/hooks';
import { useReportsStore } from 'store/slices';
import { Col, IReportInstanceModel, Loading, ReportStatusName, Row, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportHistoryForm = () => {
  const { values, setActiveInstance } = useReportEditContext();
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
      setActiveInstance(instance);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [storeReportView],
  );

  return (
    <styled.ReportHistoryForm className="report-edit-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Row alignItems="first baseline" gap="0.5em">
        <FaClockRotateLeft />
        <h2>History</h2>
      </Row>
      <Col className="report-history">
        <div className="col-1">Published On</div>
        <div className="col-2">Status</div>

        <div className="col-3">Sent On</div>
        <div className="col-4"></div>
        {instances.map((instance) => {
          return (
            <React.Fragment key={instance.id}>
              <div className="col-1">
                <Row gap="0.2em">
                  {formatDate(instance.publishedOn ?? '', false)}
                  <FaRegClock size={18} className="" />
                  {formatTime(instance.publishedOn ?? '')}
                </Row>
              </div>
              <div className="col-2 report-status">{getStatus(instance.status)}</div>

              <div className="col-3">
                <Row gap="0.2em">
                  {formatDate(instance.sentOn ?? '', false)}
                  <FaRegClock size={18} className="" />
                  {formatTime(instance.sentOn ?? '')}
                </Row>
              </div>
              <div className="col-4">
                <Action
                  icon={<FaEye />}
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
