import React from 'react';
import { FaCircleChevronDown, FaCircleChevronUp } from 'react-icons/fa6';
import { useApp } from 'store/hooks';
import { useReports } from 'store/hooks/admin';
import {
  IReportModel,
  IUserReportModel,
  Link,
  Loader,
  ReportStatusName,
  Row,
  TextArea,
} from 'tno-core';

import { ReportStatusIcon } from './ReportStatusIcon';
import { getLastSent, getNextSchedule, getStatus } from './utils';

export interface IReportCardProps {
  report: IReportModel;
}

export const ReportCard: React.FC<IReportCardProps> = ({ report: initReport }) => {
  const [, { getDashboardReport }] = useReports();
  const [{ requests }] = useApp();

  const [expand, setExpand] = React.useState(false);
  const [expandResponse, setExpandResponse] = React.useState<Record<number, boolean>>({});
  const [expandUserResponse, setExpandUserResponse] = React.useState<Record<string, boolean>>({});
  const [subscribers, setSubscribers] = React.useState<IUserReportModel[]>([]);
  const [report, setReport] = React.useState<IReportModel>(initReport);

  const instance = report.instances.length ? report.instances[0] : undefined;
  const isLoading = requests.some((r) => r.url === `get-dashboard-report-${report.id}`);

  const stringifyResponse = React.useCallback((data: any) => {
    try {
      return data ? JSON.stringify(data, undefined, `\t`) : '';
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
            {expandResponse[report.id] && (
              <div>
                <TextArea name="response" value={stringifyResponse(instance?.response)} />
              </div>
            )}
          </div>
          <h3>Subscribers</h3>
          <div className="subscribers">
            <Loader visible={isLoading} />
            {!subscribers.length && (
              <div className="subscriber">{!subscribers.length && 'No Subscribers'}</div>
            )}
            {subscribers.map((subscriber) => {
              return (
                <div key={subscriber.userId} className="subscriber">
                  <div>
                    {subscriber.email}
                    <Row>
                      <ReportStatusIcon
                        label="Link"
                        status={subscriber?.linkStatus}
                        onClick={() =>
                          setExpandUserResponse((expand) => ({
                            ...expand,
                            [`${subscriber.userId}Link`]: !expand[`${subscriber.userId}Link`],
                          }))
                        }
                      />
                    </Row>
                    <Row>
                      <ReportStatusIcon
                        label="Text"
                        status={subscriber?.textStatus}
                        onClick={() =>
                          setExpandUserResponse((expand) => ({
                            ...expand,
                            [`${subscriber.userId}Text`]: !expand[`${subscriber.userId}Text`],
                          }))
                        }
                      />
                    </Row>
                  </div>
                  {expandUserResponse[`${subscriber.userId}Link`] && (
                    <div className="response">
                      <TextArea
                        name={`subscriber-response-${subscriber.userId}-link`}
                        value={stringifyResponse(subscriber.linkResponse)}
                      />
                    </div>
                  )}
                  {expandUserResponse[`${subscriber.userId}Text`] && (
                    <div className="response">
                      <TextArea
                        name={`subscriber-response-${subscriber.userId}-text`}
                        value={stringifyResponse(subscriber.textResponse)}
                      />
                    </div>
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
