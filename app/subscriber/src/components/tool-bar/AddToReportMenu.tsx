import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaFileExport, FaPlay } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IContentModel, IReportModel, Row } from 'tno-core';

import * as styled from './styled';
import { toInstanceContent } from './utils';

export interface IAddToReportMenuProps {
  content: IContentModel[];
}
export const AddToReportMenu: React.FC<IAddToReportMenuProps> = ({ content }) => {
  const [{ updateReport, findMyReports, getReport, generateReport }] = useReports();
  const [{ myReports }] = useProfileStore();
  const [{ requests }] = useApp();
  const [activeReport, setActiveReport] = React.useState<IReportModel>();
  const [reportId, setReportId] = React.useState<number | null>(null);
  const isLoading = requests.some((r) => r.url === 'find-my-reports');

  React.useEffect(() => {
    if (!myReports.length && !isLoading) {
      findMyReports().catch(() => {});
    }

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /** Adds the content to the active report. */
  const addContentToReport = React.useCallback(
    (sectionName: string) => {
      if (!activeReport) {
        return;
      }

      const convertedContent = toInstanceContent(
        content,
        activeReport.instances[0].id,
        sectionName,
        0,
      );
      const update = (report: IReportModel) => {
        updateReport(report, true)
          .then(() => toast.success(`${content.length} storie(s) have been added to report.`))
          .catch(() => {});
      };

      if (!!activeReport.instances.length) {
        // get the latest instance content and append to it
        // 0 will always be the latest instance
        const instContent = [...activeReport.instances[0].content, ...convertedContent];

        // find the latest instance and replace the content with old content appended with new
        // this should handle create new instance if there is no instance
        const instances = activeReport.instances.map((inst, index) => {
          if (index === 0) {
            return {
              ...inst,
              content: instContent,
            };
          } else {
            return inst;
          }
        });
        const report = {
          ...activeReport,
          instances: instances,
        };
        update(report);
      }
    },
    [activeReport, content, updateReport],
  );

  // ensure no concurrency errors rather than getting from profile store
  React.useEffect(() => {
    if (reportId) {
      getReport(reportId, true)
        .then(async (report) => {
          setActiveReport(report);
          // check for instances and if the report has been sent
          if (!report?.instances?.length || !!report?.instances[0]?.sentOn) {
            const result = await generateReport(reportId);
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
        <FaFileExport /> <span>SELECT REPORT</span>
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
          {activeReport?.sections.map((section) => (
            <Row
              key={section.id}
              className="report-item"
              onClick={() => addContentToReport(section.name)}
            >
              {section.settings.label}
            </Row>
          ))}
        </div>
      </TooltipMenu>
    </styled.AddToMenu>
  );
};
