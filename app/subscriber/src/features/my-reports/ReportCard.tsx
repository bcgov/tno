import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import { formatDate } from 'features/utils';
import React from 'react';
import { FaChartPie, FaNewspaper, FaPen, FaTrashCan } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { useApp, useReports } from 'store/hooks';
import { Col, IReportModel, ReportSectionTypeName, Row, Spinner } from 'tno-core';

import { ReportKindIcon } from './components';
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

  const fetchReport = React.useCallback(
    async (id: number) => {
      try {
        return await getReport(id);
      } catch {}
    },
    [getReport],
  );

  const isLoading = requests.some((r) => r.url === `get-report-${report.id}`);

  return (
    <Section
      key={report.id}
      className={`report-card${className ? ` ${className}` : ''}`}
      icon={
        report.sections.some(
          (section) => section.sectionType === ReportSectionTypeName.MediaAnalytics,
        ) ? (
          <FaChartPie />
        ) : (
          <FaNewspaper />
        )
      }
      label={
        <Row gap="1rem" alignItems="center" justifyContent="space-between">
          {report.name}
          <ReportKindIcon report={report} />
        </Row>
      }
      actions={
        <Button onClick={() => onClick?.(report)}>
          View report <FaPen />
        </Button>
      }
      onChange={async (open) => {
        if (open) {
          await fetchReport(report.id);
        }
      }}
    >
      <Row gap="1rem">
        <Col flex="1" gap="1rem">
          <div>
            <div>
              <h3 className="upper">Report Description</h3>
              <p>{report.description ? report.description : 'NO DESCRIPTION PROVIDED'}</p>
            </div>
          </div>
          <div>
            <h3 className="upper">Status</h3>
            <Row>
              <Col>Last sent:</Col>
              <Col flex="1">{formatDate(getLastSent(report), true)}</Col>
            </Row>
            {report.instances.length > 0 && (
              <Row>
                <Col>Draft created:</Col>
                <Col flex="1">{formatDate(report.instances[0].publishedOn ?? '', true)}</Col>
              </Row>
            )}
          </div>
        </Col>
        <Col flex="1" gap="1rem">
          <div>
            <h3 className="upper">Settings</h3>
            <Link to={`/reports/${report.id}`}>edit settings</Link>
          </div>
          {report.events.map((schedule, index) => {
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
                  {schedule.settings.autoSend && <Col>autosend</Col>}
                </Row>
                <Row>
                  <Col flex="1">Next scheduled send:</Col>
                  <Col flex="1" alignItems="flex-end">
                    {formatDate(calcNextScheduleSend(schedule) ?? '', true)}
                  </Col>
                </Row>
              </div>
            );
          })}
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
    </Section>
  );
};
