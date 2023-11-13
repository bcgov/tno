import React from 'react';
import { FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { FaFileArrowUp } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup, useLookupOptions, useNotifications, useReports } from 'store/hooks';
import {
  ContentTypeName,
  IOptionItem,
  Modal,
  OptionItem,
  replaceQueryParams,
  Settings,
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
  const navigate = useNavigate();
  const [{ filterPaper: filter }, { storeFilterPaper }] = useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [{ publishReport }] = useReports();
  const [{ publishNotification }] = useNotifications();
  const [{ settings }] = useLookup();
  const { toggle, isShowing } = useModal();

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [morningReportId, setMorningReportId] = React.useState('');
  const [frontPageImagesReportId, setFrontPageImagesReportId] = React.useState('');
  const [topStoryAlertId, setTopStoryAlertId] = React.useState('');
  const [sendInfo, setSendInfo] = React.useState<IReportInfo>();

  React.useEffect(() => {
    setMorningReportId(settings.find((s) => s.name === Settings.MorningReport)?.value ?? '');
    setFrontPageImagesReportId(
      settings.find((s) => s.name === Settings.FrontPageImagesReport)?.value ?? '',
    );
    setTopStoryAlertId(settings.find((s) => s.name === Settings.TopStoryAlert)?.value ?? '');
  }, [settings]);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

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
                if (!e.ctrlKey) navigate(route);
                else window.open(route, '_blank');
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
                if (!e.ctrlKey) navigate(route);
                else window.open(route, '_blank');
              } else toast.error(`Configuration setting "${Settings.MorningReport}" is missing.`);
            }}
          />
        </ToolBarSection>
        <ToolBarSection label="Send">
          <FaFileImage
            className="action-button btn-preview"
            title="Front Page Images"
            onClick={(e) => {
              if (frontPageImagesReportId) {
                setSendInfo({
                  name: 'Front Page Images',
                  value: +frontPageImagesReportId,
                  action: 'report',
                });
                toggle();
              } else
                toast.error(
                  `Configuration setting "${Settings.FrontPageImagesReport}" is missing.`,
                );
            }}
          />
          <FaFileArrowUp
            className="action-button btn-preview"
            title="Top Stories"
            onClick={(e) => {
              setSendInfo({
                name: 'Top Stories',
                value: +topStoryAlertId,
                action: 'notification',
              });
              toggle();
            }}
          />
          <FaFileInvoice
            className="action-button btn-preview"
            title="Morning Report"
            onClick={(e) => {
              if (morningReportId) {
                setSendInfo({
                  name: 'Morning',
                  value: +morningReportId,
                  action: 'report',
                });
                toggle();
              } else toast.error(`Configuration setting "${Settings.MorningReport}" is missing.`);
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
