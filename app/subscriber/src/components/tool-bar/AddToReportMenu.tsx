import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaFileExport, FaPlay } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IContentModel, IReportModel, Row } from 'tno-core';

import * as styled from './styled';
import { newReportInstance, toInstanceContent } from './utils';

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
      if (!activeReport) {
        return;
      }

      const convertedContent = toInstanceContent(content, activeReport, section);
      const update = (report: IReportModel) => {
        updateReport(report, true)
          .then(() => toast.success(`${content.length} storie(s) have been added to report.`))
          .catch(() => {});
      };

      if (!!activeReport.instances.length) {
        // get the latest instance content and append to it
        const instContent = [
          ...activeReport.instances[activeReport.instances.length - 1].content,
          ...convertedContent,
        ];

        // find the latest instance and replace the content with old content appended with new
        // this should handle create new instance if there is no instance
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
        update(report);
      }

      if (!activeReport.instances.length) {
        const newInstance = newReportInstance(activeReport, convertedContent);
        const report = {
          ...activeReport,
          instances: [newInstance],
        };
        update(report);
      }
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
                  data-tooltip-id={`tooltip-add-to-section`}
                >
                  {report.name} {!!report.sections.length && <FaPlay className="expand-sections" />}
                </Row>
              </>
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
