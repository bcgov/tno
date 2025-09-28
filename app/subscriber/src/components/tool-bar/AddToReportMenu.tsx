import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaAngleRight, FaSearch } from 'react-icons/fa';
import { FaFileExport, FaSpinner } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReports } from 'store/hooks';
import {
  Col,
  IContentModel,
  IReportModel,
  Link,
  Loader,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import * as styled from './styled';
import { toInstanceContentPayload } from './utils';

export interface IAddToReportMenuProps {
  content: IContentModel[];
  // Callback to clear the selected content.
  onClear?: () => void;
}
export const AddToReportMenu: React.FC<IAddToReportMenuProps> = ({ content, onClear }) => {
  const [{ myReports }, { addContentToReport, findMyReports }] = useReports();
  const [{ requests }] = useApp();

  const [activeReport, setActiveReport] = React.useState<IReportModel>();
  const [inProgress, setInProgress] = React.useState({ sectionName: '', value: false });
  const [searchQuery, setSearchQuery] = React.useState('');
  const [showSearchInput, setShowSearchInput] = React.useState(false);

  const isLoading = requests.some(
    (r) => ['find-my-reports', 'generate-report'].includes(r.url) || r.group.includes('get-report'),
  );
  const isAdding = requests.some((r) => r.url === 'add-content-to-report');

  React.useEffect(() => {
    if (!myReports.length) {
      findMyReports().catch(() => {});
    }
    // Only do this on initialization.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const setActiveSectionSearch = React.useCallback(() => {
    setShowSearchInput((showSearchInput) => !showSearchInput);
    setSearchQuery('');
  }, []);

  /** Adds the content to the active report. */
  const handleAddToReport = React.useCallback(
    async (report: IReportModel, sectionName: string, content: IContentModel[]) => {
      try {
        setInProgress({ sectionName, value: true });
        const payload = toInstanceContentPayload(content, 0, sectionName, 0);
        const result = await addContentToReport(report.id, payload);

        const addedCount = result?.added?.length ?? content.length;
        toast.success(() => (
          <div>
            {addedCount} stories added to report:
            <Link to={`reports/${report.id}/content`}>{report.name}</Link>
          </div>
        ));
        onClear?.();
        setSearchQuery('');
      } catch {
        // Errors are handled globally.
      } finally {
        setInProgress({ sectionName: '', value: false });
        setSearchQuery('');
      }
    },
    [addContentToReport, onClear],
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
                <Row className="report-item" data-tooltip-id={`tooltip-add-to-section`}>
                  <FaFileExport
                    className={`report-icon ${activeReport?.id === report.id && 'expanded'}`}
                  />
                  <div
                    className="report-name"
                    onClick={() => {
                      // allow user to toggle close list of sections
                      if (activeReport?.id === report.id) setActiveReport(undefined);
                      else setActiveReport(report);
                    }}
                  >
                    {report.name}
                  </div>
                  <FaSearch
                    className={`report-search-icon ${activeReport?.id === report.id && 'expanded'}`}
                    onClick={setActiveSectionSearch}
                  />
                </Row>
                <Show visible={!!activeReport && activeReport.id === report.id}>
                  <Show visible={showSearchInput}>
                    <input
                      className={`section-search-input`}
                      type="text"
                      placeholder="Search sections..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      autoFocus={showSearchInput}
                    />
                  </Show>
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
