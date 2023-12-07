import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaFileExport, FaPlay } from 'react-icons/fa6';
import { useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IContentModel, IReportInstanceContentModel, IReportModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IAddToReportMenuProps {
  content: IContentModel[];
}

export const AddToReportMenu: React.FC<IAddToReportMenuProps> = ({ content }) => {
  const [{ updateReport, findMyReports }] = useReports();
  const [{ myReports }] = useProfileStore();
  const [activeReport, setActiveReport] = React.useState<IReportModel | null>(null);
  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on init.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /** Adds the content to the active report. */
  const addContentToReport = React.useCallback(
    (section: string) => {
      if (!activeReport || !activeReport.instances.length) {
        return;
      }

      // convert the content to an instance content
      const convertedContent: IReportInstanceContentModel = {
        contentId: content[0].id,
        content: content[0],
        sectionName: section,
        instanceId: activeReport.instances[activeReport.instances.length - 1].id,
        createdBy: content[0].createdBy,
        createdOn: content[0].createdOn,
        sortOrder: 0,
      };

      // get the latest instance content and append to it
      const instContent = [
        ...activeReport.instances[activeReport.instances.length - 1].content,
        convertedContent,
      ];

      // find the latest instance and replace the content with old content appended with new
      const instances = activeReport.instances.map((inst, index) => {
        if (index === activeReport.instances.length - 1) {
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
      updateReport(report, true).catch(() => {});
    },
    [activeReport, content, updateReport],
  );

  return (
    <styled.AddToMenu>
      <div data-tooltip-id="tooltip-add-to-report" className="action">
        <FaFileExport /> SELECT REPORT
        <TooltipMenu clickable openOnClick id="tooltip-add-to-report" place="bottom">
          <Row className="report">
            <FaFileExport /> SELECT REPORT...
          </Row>
          <div className="list">
            {myReports.map((report) => (
              <>
                <Row
                  key={report.id}
                  className="report-item"
                  onClick={() => setActiveReport(report)}
                  data-tooltip-id={`tooltip-add-to-section-${report.id}`}
                >
                  {report.name} {!!report.sections.length && <FaPlay className="expand-sections" />}
                </Row>
              </>
            ))}
          </div>
        </TooltipMenu>
      </div>
      <TooltipMenu
        place="right"
        id={`tooltip-add-to-section-${activeReport?.id}`}
        clickable
        openOnClick
      >
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
