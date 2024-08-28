import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import { formatDate } from 'features/utils';
import React from 'react';
import { FaChartPie, FaEye, FaGear, FaTrashCan } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useApp, useReports } from 'store/hooks';
import { Col, IReportModel, ReportSectionTypeName, Row, Show, Spinner } from 'tno-core';

import { ReportKindIcon } from './components';
import { ReportStatus } from './ReportStatus';
import { calcNextScheduleSend, getLastSent } from './utils';

export interface IReportCardProps {
  /** The report to display on this card. */
  report: IReportModel;
  /** Event fires when header is clicked. */
  onClick?: (report?: IReportModel) => void;
  /** Event fires when user requests to delete report. This event does not delete the report itself. */
  onDelete?: (report: IReportModel) => void;
  /** Class name */
  className?: string;
}

/**
 * A card to display report information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportCard: React.FC<IReportCardProps> = ({
  className,
  report,
  onClick,
  onDelete,
}) => {
  const [, { getReport }] = useReports();
  const [{ requests }] = useApp();
  const navigate = useNavigate();

  const instance = report.instances.length ? report.instances[0] : undefined;

  const fetchReport = React.useCallback(
    async (id: number) => {
      try {
        return await getReport(id);
      } catch {}
    },
    [getReport],
  );

  const isLoading = requests.some((r) => r.url === `get-report-${report.id}`);

  const includesMediaAnalytics = React.useMemo(() => {
    return report.sections.some(
      (section) => section.sectionType === ReportSectionTypeName.MediaAnalytics,
    );
  }, [report.sections]);

  return (
    <Section
      key={report.id}
      className={`report-card${className ? ` ${className}` : ''}`}
      label={
        <Row gap="1rem" alignItems="center">
          <ReportKindIcon report={report} />
          <span className="report-name">{report.name}</span>
        </Row>
      }
      actions={
        <>
          <Button onClick={() => onClick?.(report)}>
            View report <FaEye />
          </Button>
          <Button onClick={() => navigate(`/reports/${report.id}`)}>
            <FaGear />
          </Button>
        </>
      }
      onChange={async (open) => {
        if (open) {
          await fetchReport(report.id);
        }
      }}
    >
      {() => (
        <Row gap="1rem">
          <Col flex="1" gap="1rem">
            <div>
              <Show visible={!!report.description}>
                <h3 className="upper">Report Description</h3>
                <p>{report.description}</p>
              </Show>
            </div>
            {includesMediaAnalytics && (
              <Row className="media-analytics">
                <FaChartPie />
                <span>Includes Media Analytics</span>
              </Row>
            )}
            <div>
              <h3 className="upper">Status</h3>
              <Show visible={!!instance}>
                <Row>
                  <Col>Status:</Col>
                  <Col>
                    <ReportStatus status={instance?.status} />
                  </Col>
                </Row>
              </Show>
              <Row>
                <Col>Last sent:</Col>
                <Col flex="1">{getLastSent(report)}</Col>
              </Row>
              {report.instances.length > 0 && (
                <Row>
                  <Col>Draft created:</Col>
                  <Col flex="1">{formatDate(report.instances[0].publishedOn ?? '', true)}</Col>
                </Row>
              )}
            </div>
          </Col>
          <Col flex="1" gap="1rem" className="report-card-schedule">
            <Row className="report-type-info">
              <ReportKindIcon includeDescription report={report} />
            </Row>
            <Show visible={report.events.some((e) => e.isEnabled)}>
              {report.events.map((schedule, index) => {
                const nextSend = formatDate(calcNextScheduleSend(report, schedule) ?? '', true);
                return (
                  <div key={`${schedule.id}-${index}`}>
                    <Row>
                      <h3>Schedule {index + 1}</h3>
                      <Col flex="1">
                        {schedule.isEnabled ? (
                          <span className="report-schedule-enabled">enabled</span>
                        ) : (
                          <span className="report-schedule-disabled">disabled</span>
                        )}
                      </Col>
                    </Row>
                    <Row>
                      <div className="next-scheduled">
                        Next scheduled {schedule.settings.autoSend ? 'send' : 'run'}:
                      </div>
                      <div>{!!nextSend ? nextSend : 'n/a'}</div>
                    </Row>
                  </div>
                );
              })}
            </Show>
            <Row justifyContent="flex-end">
              <Action
                icon={<FaTrashCan />}
                onClick={() => {
                  onDelete?.(report);
                }}
              />
            </Row>
          </Col>
          {isLoading && (
            <Col alignContent="center" justifyContent="center">
              <Spinner />
            </Col>
          )}
        </Row>
      )}
    </Section>
  );
};
