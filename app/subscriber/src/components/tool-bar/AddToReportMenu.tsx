import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaFileExport, FaPlay } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReports } from 'store/hooks';
import { IContentModel, IReportModel, Link, ReportSectionTypeName, Row } from 'tno-core';

import * as styled from './styled';
import { toInstanceContent } from './utils';

export interface IAddToReportMenuProps {
  content: IContentModel[];
  // Callback to clear the selected content.
  onClear?: () => void;
}
export const AddToReportMenu: React.FC<IAddToReportMenuProps> = ({ content, onClear }) => {
  const [{ myReports }, { addContentToReport, findMyReports, getReport, generateReport }] =
    useReports();
  const [{ requests }] = useApp();
  const [activeReport, setActiveReport] = React.useState<IReportModel>();
  const [reportId, setReportId] = React.useState<number | null>(null);
  const isLoading = requests.some((r) => r.url === 'find-my-reports');

  React.useEffect(() => {
    if (!myReports.length && !isLoading) {
      findMyReports().catch(() => {});
    }
    // Only do this on initialization.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /** Adds the content to the active report. */
  const handleAddToReport = React.useCallback(
    async (sectionName: string) => {
      if (!activeReport) return;

      try {
        var instance = activeReport.instances.length ? activeReport.instances[0] : undefined;
        if (!instance || instance.sentOn) {
          toast.error('Unable to generate a new report');
          return;
        }

        const convertedContent = toInstanceContent(
          content,
          activeReport.instances[0].id,
          sectionName,
          0,
        );

        const instances = activeReport.instances.map((i) =>
          i.id === instance!.id ? { ...i, content: [...i.content, ...convertedContent] } : i,
        );

        const report = {
          ...activeReport,
          instances: instances,
        };

        await addContentToReport(report.id, convertedContent);

        toast.success(() => (
          <div>
            {content.length} stories added to report:
            <Link to={`reports/${report.id}/content`}>{report.name}</Link>
          </div>
        ));
        onClear?.();
      } catch {}
    },
    [activeReport, content, addContentToReport, onClear],
  );

  // ensure no concurrency errors rather than getting from profile store
  React.useEffect(() => {
    if (reportId) {
      getReport(reportId, false)
        .then(async (report) => {
          setActiveReport(report);
          // check for instances and if the report has been sent
          if (!report?.instances?.length || !!report?.instances[0]?.sentOn) {
            const result = await generateReport(reportId, true);
            setActiveReport(result);
          }
        })
        .catch(() => {});
    }
    // only want to get report when reportId changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [reportId]);

  return (
    <styled.AddToMenu>
      <div data-tooltip-id="tooltip-add-to-report" className="action">
        <FaFileExport /> <span>ADD TO REPORT</span>
        <TooltipMenu clickable openOnClick id="tooltip-add-to-report" place="bottom">
          <Row className="report">
            <FaFileExport /> SELECT REPORT...
          </Row>
          <div className="list">
            {myReports.map((report) => (
              <Row
                key={report.id}
                className="report-item"
                onClick={() => setReportId(report.id)}
                data-tooltip-id={`tooltip-add-to-section`}
              >
                {report.name} {!!report.sections.length && <FaPlay className="expand-sections" />}
              </Row>
            ))}
          </div>
        </TooltipMenu>
      </div>
      <TooltipMenu place="right" id={`tooltip-add-to-section`} clickable openOnClick>
        <div className="list">
          <div className="row-title">SECTIONS:</div>
          {activeReport?.sections.map(
            (section) =>
              section.sectionType === ReportSectionTypeName.Content && (
                <Row
                  key={section.id}
                  className="report-item"
                  onClick={() => handleAddToReport(section.name).catch(() => {})}
                >
                  {section.settings.label}
                </Row>
              ),
          )}
        </div>
      </TooltipMenu>
    </styled.AddToMenu>
  );
};
