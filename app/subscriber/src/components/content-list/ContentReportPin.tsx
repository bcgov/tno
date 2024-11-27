import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaThumbtack } from 'react-icons/fa';
import { Link } from 'react-router-dom';
import { useProfileStore } from 'store/slices';
import { IReportModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IContentReportPinProps {
  /** The content ID */
  contentId: number;
}

/**
 * Displays a pin and tooltip if the specified 'contentId' is in any of the current user's reports.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentReportPin = ({ contentId }: IContentReportPinProps) => {
  const [{ reportContent, myReports }] = useProfileStore();

  const [reports, setReports] = React.useState<IReportModel[]>([]);

  React.useEffect(() => {
    setReports(
      Object.keys(reportContent)
        .filter((reportId) => {
          const content = reportContent[+reportId];
          return content.some((c) => c === contentId);
        })
        .map((reportId) => myReports.find((r) => r.id === +reportId)!)
        .filter((report) => report !== undefined),
    );
  }, [contentId, myReports, reportContent]);

  if (!reports.length) return <div className="icon-placeholder" />;

  return (
    <styled.ContentReportPin className="content-report-pin">
      <img
        src={`/assets/report_pin.svg`}
        alt="report-pin"
        data-tooltip-id={`report-pin-${contentId}`}
        className="pin-icon"
      />
      <TooltipMenu
        clickable
        place="right"
        id={`report-pin-${contentId}`}
        className="content-report-pin-tooltip"
      >
        <Row nowrap gap="0.5rem">
          <FaThumbtack />
          Appears in {reports.length} reports.
        </Row>
        {reports.map((report) => (
          <div key={report.id} className="report-link">
            <Link to={`/reports/${report.id}/content`}>{report.name}</Link>
          </div>
        ))}
      </TooltipMenu>
    </styled.ContentReportPin>
  );
};
