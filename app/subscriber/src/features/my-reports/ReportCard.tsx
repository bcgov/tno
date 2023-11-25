import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import moment from 'moment';
import React from 'react';
import {
  FaChartPie,
  FaFileLines,
  FaGear,
  FaPen,
  FaRegCalendarDays,
  FaTrashCan,
} from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { useReports } from 'store/hooks';
import { Col, IReportModel, Row } from 'tno-core';

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

  const fetchReport = React.useCallback(
    async (id: number) => {
      try {
        await getReport(id);
      } catch {}
    },
    [getReport],
  );

  return (
    <Section
      key={report.id}
      icon={
        report.sections.some((section) => section.settings.showCharts) ? (
          <FaChartPie />
        ) : (
          <FaFileLines />
        )
      }
      label={report.name}
      actions={
        <>
          <Button onClick={() => navigate(`/reports/${report.id}/edit`)}>
            Edit <FaPen />
          </Button>
          <Action icon={<FaGear />} onClick={() => navigate(`/reports/${report.id}/settings`)} />
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
      <Col gap="1rem">
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
              <Action label={'VIEW'} onClick={() => navigate(`/reports/${report.id}/view`)} />
            </Row>
          ) : (
            'NA'
          )}
        </div>
      </Col>
    </Section>
  );
};
