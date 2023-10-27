import React from 'react';
import { FaFileImage, FaFileInvoice } from 'react-icons/fa';
import { FaFileArrowUp } from 'react-icons/fa6';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import { storeContentFilterAdvanced } from 'store/slices';
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

import { AdvancedSearchKeys } from '../constants';
import { IContentListFilter } from '../interfaces';
import { CreateNewSection } from '../list-view/components/tool-bar/filter';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { getPreviewReportRoute } from '../utils';
import { AdvancedFilter, ContentFilter } from './components';
import { defaultPaperFilter } from './constants';
import { IReportInfo } from './interfaces';
import * as styled from './styled';

export interface IPaperFilterProps {
  /** Event fired when search is executed. */
  onSearch: (filter: IContentListFilter) => void;
}

/**
 * Provides a filter for the papers.
 * @param param0 Component properties.
 * @returns Component.
 */
export const PaperFilter: React.FC<IPaperFilterProps> = ({ onSearch }) => {
  const navigate = useNavigate();
  const [{ filterPaper: filter, filterAdvanced }, { storeFilterPaper }] = useContent();
  const [{ productOptions: pOptions }] = useLookupOptions();
  const [{ sources, settings }] = useLookup();
  const { toggle, isShowing } = useModal();

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [morningReportId, setMorningReportId] = React.useState('');
  const [frontPageImagesReportId, setFrontPageImagesReportId] = React.useState('');
  const [sendInfo, setSendInfo] = React.useState<IReportInfo>({
    name: 'Morning Report',
    reportId: 0,
  });

  React.useEffect(() => {
    // Extract query string values and place them into redux store.
    if (!window.location.search) {
      replaceQueryParams(defaultPaperFilter(sources), { includeEmpty: false });
    }
    storeFilterPaper(
      queryToFilter(
        {
          ...defaultPaperFilter(sources),
        },
        window.location.search,
      ),
    );
    storeContentFilterAdvanced(
      queryToFilterAdvanced(
        { ...filterAdvanced, fieldType: AdvancedSearchKeys.Headline },
        window.location.search,
      ),
    );
    // Only want this to run on the first load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    setMorningReportId(settings.find((s) => s.name === Settings.MorningReport)?.value ?? '');
    setFrontPageImagesReportId(
      settings.find((s) => s.name === Settings.FrontPageImagesReport)?.value ?? '',
    );
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
    <styled.PaperFilter>
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
              setSendInfo({ name: 'Front Page Images', reportId: +frontPageImagesReportId });
              toggle();
            }}
          />
          <FaFileArrowUp
            className="action-button btn-preview"
            title="Top Stories"
            onClick={(e) => {
              setSendInfo({ name: 'Top Stories', reportId: +frontPageImagesReportId });
              toggle();
            }}
          />
          <FaFileInvoice
            className="action-button btn-preview"
            title="Morning Report"
            onClick={(e) => {
              setSendInfo({ name: 'Morning', reportId: +frontPageImagesReportId });
              toggle();
            }}
          />
        </ToolBarSection>
      </ToolBar>
      <Modal
        headerText={`${sendInfo.name} Report`}
        body={`Do you want to send out the ${sendInfo.name} report to subscribers?`}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Send"
        onConfirm={async () => {
          try {
            // await api.deleteAction(action);
            // toast.success(`${action.name} has successfully been deleted.`);
            // navigate('/admin/actions');
          } finally {
            toggle();
          }
        }}
      />
    </styled.PaperFilter>
  );
};
