import React from 'react';
import { FaFileInvoice } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup, useLookupOptions } from 'store/hooks';
import { storeContentFilterAdvanced } from 'store/slices';
import {
  ContentTypeName,
  IOptionItem,
  OptionItem,
  replaceQueryParams,
  ToolBar,
  ToolBarSection,
} from 'tno-core';

import { CreateNewSection } from '../list-view/components/tool-bar/filter';
import { AdvancedSearchKeys } from '../list-view/constants';
import { queryToFilter, queryToFilterAdvanced } from '../list-view/utils';
import { AdvancedFilter, ContentFilter } from './components';
import { defaultPaperFilter } from './constants';
import { IPaperFilter } from './interfaces';
import * as styled from './styled';

export interface IPaperFilterProps {
  /** Event fired when search is executed. */
  onSearch: (filter: IPaperFilter) => void;
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

  const [, setProductOptions] = React.useState<IOptionItem[]>([]);
  const [morningReportId, setMorningReportId] = React.useState('');

  const reportPreviewRoute = `/reports/${morningReportId}/preview`;

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
    const id = settings.find((s) => s.name === 'MorningReport')?.value;
    if (id) setMorningReportId(id);
  }, [settings]);

  React.useEffect(() => {
    setProductOptions([new OptionItem<number>('Any', 0), ...pOptions]);
  }, [pOptions]);

  const onFilterChange = (filter: IPaperFilter) => {
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
          <FaFileInvoice
            className="action-button btn-preview"
            onClick={(e) => {
              if (morningReportId) {
                if (!e.ctrlKey) navigate(reportPreviewRoute);
                else window.open(reportPreviewRoute, '_blank');
              } else toast.error('Configuration setting "MorningReport" is missing.');
            }}
          />
        </ToolBarSection>
      </ToolBar>
    </styled.PaperFilter>
  );
};
