import React from 'react';
import { FaCircleChevronDown, FaCircleChevronUp } from 'react-icons/fa6';
import { useReports } from 'store/hooks/admin';
import { IReportModel, IUserReportModel, Link, ReportStatusName } from 'tno-core';

import { ReportStatusIcon } from './ReportStatusIcon';
import { getLastSent, getNextSchedule, getStatus } from './utils';

export interface IReportCardProps {
  report: IReportModel;
}

export const ReportCard: React.FC<IReportCardProps> = ({ report: initReport }) => {
  const [, { getDashboardReport }] = useReports();

  const [expand, setExpand] = React.useState(false);
  const [expandResponse, setExpandResponse] = React.useState<Record<number, boolean>>({});
  const [expandUserResponse, setExpandUserResponse] = React.useState<Record<number, boolean>>({});
  const [subscribers, setSubscribers] = React.useState<IUserReportModel[]>([]);
  const [report, setReport] = React.useState<IReportModel>(initReport);

  const instance = report.instances.length ? report.instances[0] : undefined;

  const stringifyResponse = React.useCallback((data: any) => {
    try {
      return data ? JSON.stringify(data) : '';
    } catch {
      return '';
    }
  }, []);

  React.useEffect(() => {
    setSubscribers(report.subscribers.filter((s) => s.isSubscribed));
  }, [report.subscribers]);

  React.useEffect(() => {
    if (expand)
      getDashboardReport(report.id)
        .then((result) => {
          setReport(result);
        })
        .catch(() => {});
    // Only fetch when expanding report.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [expand]);

  return (
    <div className={`report-card`}>
      <div>
        <Link to={`/admin/reports/${report.id}`}>{report.name}</Link>
      </div>
      <div>{report.owner?.email}</div>
      <div className={`${instance?.status === ReportStatusName.Failed ? 'failed' : ''}`}>
        {getStatus(instance?.status)}
      </div>
      <div>{getLastSent(report)}</div>
      <div>{getNextSchedule(report)}</div>
      <div className="buttons">
        <div onClick={() => setExpand((value) => !value)}>
          {expand ? <FaCircleChevronUp /> : <FaCircleChevronDown />}
        </div>
      </div>
      {expand && (
        <div className="full-width">
          <div className="response">
            <div
              className="buttons left"
              onClick={() =>
                setExpandResponse((value) => ({ ...value, [report.id]: !value[report.id] }))
              }
            >
              <h3>CHES Response</h3>
              <div>
                {expandResponse[report.id] ? <FaCircleChevronUp /> : <FaCircleChevronDown />}
              </div>
            </div>
            {expandResponse[report.id] && <div>{stringifyResponse(instance?.response)}</div>}
          </div>
          <h3>Subscribers</h3>
          <div className="subscribers">
            {!subscribers.length && (
              <div className="subscriber">{!subscribers.length && 'No Subscribers'}</div>
            )}
            {subscribers.map((subscriber) => {
              return (
                <div key={subscriber.userId} className="subscriber">
                  <div>
                    {subscriber.email}
                    <ReportStatusIcon
                      status={subscriber?.status}
                      onClick={() =>
                        setExpandUserResponse((expand) => ({
                          ...expand,
                          [subscriber.userId]: !expand[subscriber.userId],
                        }))
                      }
                    />
                  </div>
                  {expandUserResponse[subscriber.userId] && (
                    <div className="response">{stringifyResponse(subscriber.response)}</div>
                  )}
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};
