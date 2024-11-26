import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaAngleRight, FaSearch } from 'react-icons/fa';
import { FaFileExport, FaSpinner } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  IContentModel,
  IReportModel,
  Link,
  Loader,
  ReportSectionTypeName,
  ReportStatusName,
  Row,
  Show,
} from 'tno-core';

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
  const [inProgress, setInProgress] = React.useState({ sectionName: '', value: false });
  const [searchQuery, setSearchQuery] = React.useState('');
  const [showSearchInput, setShowSearchInput] = React.useState(false);

  const isLoading = requests.some(
    (r) => ['find-my-reports', 'generate-report'].includes(r.url) || r.group.includes('get-report'),
  );
  const isAdding = requests.some((r) => r.url === 'add-content-to-report');
  const [{ reportsFilter }] = useProfileStore();

  const [filter, setFilter] = React.useState(reportsFilter);

  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on initialization.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const setActiveSectionSearch = React.useCallback(() => {
    setShowSearchInput((prevState) => !prevState);
    console.log(showSearchInput);
  }, []);

  /** Adds the content to the active report. */
  const handleAddToReport = React.useCallback(
    async (report: IReportModel, sectionName: string, content: IContentModel[]) => {
      try {
        setInProgress({ sectionName, value: true });
        let currentReport: IReportModel | undefined = { ...report };
        let instance = currentReport.instances.length ? currentReport.instances[0] : undefined;

        if (!report.instances.length) {
          // Fetch the report because the one in memory has no instances.
          currentReport = await getReport(report.id, true);

          // The report does not have an instance, we must create one to add content.
          // This should only occur the first time after a new report is created.
          if (!currentReport?.instances.length) {
            currentReport = await generateReport(report.id, true);
          }
          instance = currentReport.instances.length ? currentReport.instances[0] : undefined;
        }

        if (
          instance &&
          [ReportStatusName.Accepted, ReportStatusName.Completed, ReportStatusName.Failed].includes(
            instance.status,
          )
        ) {
          // Start the next report.
          currentReport = await generateReport(report.id, true);
          instance = currentReport.instances.length ? currentReport.instances[0] : undefined;
        }

        if (!currentReport || !instance) {
          toast.error('Failed to generate a new report');
          return;
        }

        const convertedContent = toInstanceContent(content, instance.id, sectionName, 0);
        await addContentToReport(currentReport.id, convertedContent);

        toast.success(() => (
          <div>
            {content.length} stories added to report:
            <Link to={`reports/${currentReport!.id}/content`}>{currentReport!.name}</Link>
          </div>
        ));
        onClear?.();
      } catch {
        // Errors are handled globally.
      } finally {
        setInProgress({ sectionName: '', value: false });
      }
    },
    [addContentToReport, onClear, getReport, generateReport],
  );

  return (
    <styled.AddToMenu>
      <div data-tooltip-id="tooltip-add-to-report" className="action">
        <FaFileExport /> <span>ADD TO REPORT</span>
        <TooltipMenu clickable openOnClick id="tooltip-add-to-report" place="bottom">
          <Row className="choose-report">Choose Report...</Row>
          <Col className="list">
            <Loader visible={isLoading} />
            {myReports.map((report) => (
              <Show key={report.id} visible={!!report.sections.length}>
                <Row
                  className="report-item"
                  onClick={() => {
                    // allow user to toggle close list of sections
                    if (activeReport?.id === report.id) setActiveReport(undefined);
                    else setActiveReport(report);
                  }}
                  data-tooltip-id={`tooltip-add-to-section`}
                >
                  <FaFileExport
                    className={`report-icon ${activeReport?.id === report.id && 'expanded'}`}
                  />
                  <div className="report-name">{report.name}</div>
                </Row>
                <Show visible={!!activeReport && activeReport.id === report.id}>
                  <div className={`search-section-container`}>
                    <FaSearch className={`report-search-icon`} onClick={setActiveSectionSearch} />
                    <input
                      className={`section-search-input ${showSearchInput}`}
                      type="text"
                      placeholder="Search sections..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      autoFocus={showSearchInput}
                    />
                  </div>
                  <div className={`section-list`}>
                    {activeReport?.sections.map(
                      (section) =>
                        section.sectionType === ReportSectionTypeName.Content &&
                        section.settings.label
                          .toLowerCase()
                          .includes(searchQuery.toLowerCase()) && (
                          <Row
                            key={section.id}
                            className="section"
                            onClick={() =>
                              !isAdding &&
                              content.length &&
                              inProgress.sectionName !== section.name &&
                              handleAddToReport(report, section.name, content)
                            }
                          >
                            <FaAngleRight className="active-section" />
                            {section.settings.label}
                            {section.name === inProgress.sectionName && inProgress.value && (
                              <FaSpinner className="spinner" />
                            )}
                          </Row>
                        ),
                    )}
                  </div>
                </Show>
              </Show>
            ))}
          </Col>
        </TooltipMenu>
      </div>
    </styled.AddToMenu>
  );
};
