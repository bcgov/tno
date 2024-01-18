import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import moment from 'moment';
import React from 'react';
import {
  FaChartPie,
  FaFileLines,
  FaGear,
  FaNewspaper,
  FaPen,
  FaRegCalendarDays,
  FaTrashCan,
} from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useApp, useReports } from 'store/hooks';
import { Col, IReportModel, ReportSectionTypeName, Row, Spinner } from 'tno-core';

import { calcNextSend, getLastSent } from './utils';

export interface IReportCardProps {
  /** The report to display on this card. */
  report: IReportModel;
  /** Event fires when user requests to delete report. This event does not delete the report itself. */
  onDelete?: (report: IReportModel) => void;
}

/**
 * A card to display report information.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportCard: React.FC<IReportCardProps> = ({ report, onDelete }) => {
  const navigate = useNavigate();
  const [{ getReport }] = useReports();
  const [{ requests }] = useApp();

  const fetchReport = React.useCallback(
    async (id: number) => {
      try {
        await getReport(id);
      } catch {}
    },
    [getReport],
  );

  const isLoading = requests.some((r) => r.url === `get-report-${report.id}`);

  return (
    <Section
      key={report.id}
      icon={
        report.sections.some(
          (section) => section.sectionType === ReportSectionTypeName.MediaAnalytics,
        ) ? (
          <FaChartPie />
        ) : (
          <FaNewspaper />
        )
      }
      label={report.name}
      actions={
        <>
          <Button onClick={() => navigate(`/reports/${report.id}/edit`)}>
            Edit <FaPen />
          </Button>
          <Action icon={<FaGear />} onClick={() => navigate(`/reports/${report.id}`)} />
          <Action
            icon={<FaTrashCan />}
            onClick={() => {
              onDelete?.(report);
            }}
          />
        </>
      }
      onChange={(open) => open && fetchReport(report.id)}
    >
      <Row>
        <Col gap="1rem" flex="1">
          {report.description && <div>{report.description}</div>}
          <div className="report-schedule">
            <div>
              <FaRegCalendarDays />
            </div>
            <Col>
              <Row gap="1rem" className="fs1">
                <label className="b7">Last sent:</label>
                <span>{getLastSent(report)}</span>
              </Row>
              <Row gap="1rem" className="fs1">
                <label className="b7">Next scheduled send:</label>
                <span>{calcNextSend(report)}</span>
              </Row>
            </Col>
          </div>
          <div className="report-instance">
            <div>
              <FaFileLines />
            </div>
            <label className="b7 fs1">Current instance created:</label>
            {report.instances.length ? (
              <Row gap="1rem" alignItems="center">
                <span className="fs1">
                  {moment(report.instances[0].publishedOn).format('DD-MMM-YYYY h:mm:ss a')}
                </span>
                <Action
                  label={'VIEW'}
                  onClick={() => {
                    if (report.instances.length)
                      navigate(`/report/instances/${report.instances[0].id}/view`);
                    else navigate(`/reports/${report.id}/view`);
                  }}
                />
              </Row>
            ) : (
              'NA'
            )}
          </div>
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
