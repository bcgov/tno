import React from 'react';
import { FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { FaFileArrowUp } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import {
  useContent,
  useLookupOptions,
  useNotifications,
  useReports,
  useSettings,
} from 'store/hooks';
import {
  ContentTypeName,
  IOptionItem,
  Modal,
  OptionItem,
  replaceQueryParams,
  Settings,
  Spinner,
  ToolBar,
  ToolBarSection,
  useModal,
} from 'tno-core';

import { IContentListFilter } from '../interfaces';
import { CreateNewSection } from '../list-view/components/tool-bar/filter';
import { getPreviewReportRoute } from '../utils';
import { AdvancedFilter, ContentFilter } from './components';
import { IReportInfo } from './interfaces';
import * as styled from './styled';

export interface IPaperToolbarProps {
  /** Event fired when search is executed. */
  onSearch: (filter: IContentListFilter) => void;
}

/**
 * Provides a filter for the papers.
 * @param param0 Component properties.
 * @returns Component.
 */
export const PaperToolbar: React.FC<IPaperToolbarProps> = ({ onSearch }) => {
  const [{ filterPaper: filter }, { storeFilterPaper }] = useContent();
  const [{ mediaTypeOptions }] = useLookupOptions();
  const [{ previewReport, publishReport }] = useReports();
  const [{ publishNotification }] = useNotifications();
  const { morningReportId, frontPageImagesReportId, topStoryAlertId } = useSettings();
  const { toggle, isShowing } = useModal();

  const [, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [sendInfo, setSendInfo] = React.useState<IReportInfo>();

  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    setMediaTypeOptions([new OptionItem<number>('Any', 0), ...mediaTypeOptions]);
  }, [mediaTypeOptions]);

  async function checkReportContent(reportID: number) {
    try {
      setIsLoading(true);
      const preview = await previewReport(reportID);
      if (!preview.data || !preview.data.length) {
        toast.error('Report content is empty. Please add content before sending.');
        return false;
      } else if (preview.body && preview.body === '\n') {
        toast.error(
          `Report tempalte is inactive, check the settings ReportID with value ${reportID}.`,
        );
        return false;
      } else {
        return true;
      }
    } catch (error) {
      toast.error(`Failed to preview report. ${error}`);
    } finally {
      setIsLoading(false);
    }
  }

  const onFilterChange = (filter: IContentListFilter) => {
    const newFilter = { ...filter, pageIndex: 0 };
    storeFilterPaper(newFilter);
    replaceQueryParams(newFilter, { includeEmpty: false });
  };

  return (
    <styled.PaperToolbar>
      <ToolBar>
        <CreateNewSection contentTypes={[ContentTypeName.PrintContent, ContentTypeName.Image]} />
        <ContentFilter filter={filter} onFilterChange={onFilterChange} />
        <AdvancedFilter onSearch={onSearch} filter={filter} onFilterChange={onFilterChange} />
        <ToolBarSection label="Preview">
          <FaFileImage
            className="action-button btn-preview"
            title="Front Page Images"
            onClick={(e) => {
              if (frontPageImagesReportId) {
                const route = getPreviewReportRoute(+frontPageImagesReportId);
                window.open(route, '_blank');
              } else
                toast.error(
                  `Configuration setting "${Settings.FrontPageImagesReport}" is missing.`,
                );
            }}
          />
          <FaFileInvoice
            className="action-button btn-preview"
            title="Morning Report"
            onClick={(e) => {
              if (morningReportId) {
                const route = getPreviewReportRoute(+morningReportId);
                window.open(route, '_blank');
              } else toast.error(`Configuration setting "${Settings.MorningReport}" is missing.`);
            }}
          />
        </ToolBarSection>
        <ToolBarSection label="Send">
          {isLoading ? (
            <Spinner />
          ) : (
            <FaFileImage
              className="action-button btn-preview"
              title="Front Page Images"
              onClick={(e) => {
                if (frontPageImagesReportId) {
                  setSendInfo({
                    name: 'Front Page Images',
                    value: frontPageImagesReportId,
                    action: 'report',
                  });
                  toggle();
                }
              }}
            />
          )}
          <FaFileArrowUp
            className="action-button btn-preview"
            title="Top Stories"
            onClick={(e) => {
              if (topStoryAlertId) {
                setSendInfo({
                  name: 'Top Stories',
                  value: topStoryAlertId,
                  action: 'notification',
                });
                toggle();
              }
            }}
          />
          <FaFileInvoice
            className="action-button btn-preview"
            title="Morning Report"
            onClick={(e) => {
              if (morningReportId) {
                setSendInfo({
                  name: 'Morning',
                  value: morningReportId,
                  action: 'report',
                });
                toggle();
              }
            }}
          />
        </ToolBarSection>
      </ToolBar>
      <Modal
        headerText={`${sendInfo?.name} Report`}
        body={`Do you want to send out the ${sendInfo?.name} report to subscribers?  This will send an email `}
        isShowing={isShowing && !!sendInfo}
        hide={toggle}
        type="default"
        confirmText="Yes, Send"
        onConfirm={async () => {
          try {
            if (sendInfo && sendInfo.value) {
              if (sendInfo.action === 'report') {
                await publishReport(sendInfo.value);
                toast.success(`Request to send ${sendInfo.name} report has been submitted.`);
              } else if (sendInfo.action === 'notification') {
                await publishNotification(sendInfo.value);
                toast.success(`Request to send ${sendInfo.name} notifications has been submitted.`);
              }
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.PaperToolbar>
  );
};
