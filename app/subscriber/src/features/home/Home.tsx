import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat, getFilterActions } from 'features/search-page/utils';
import { createFilterSettings } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { ActionName, generateQuery, IContentModel, IFilterSettingsModel, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch, storeHomeFilter: storeFilter },
  ] = useContent();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [isReady, setIsReady] = React.useState<boolean>(false);
  const [settings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(
      filter.startDate ?? moment().startOf('day').toISOString(),
      filter.endDate ?? moment().endOf('day').toISOString(),
    ),
  );

  const [{ actions }] = useLookup();
  const contentType = useMemo(() => {
    if (!!filter?.contentTypes?.length) return filter.contentTypes[0];
    else return 'all';
  }, [filter.contentTypes]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setContent(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  const homePage = React.useMemo(() => {
    return getFilterActions(actions)[ActionName.Homepage];
  }, [actions]);

  React.useEffect(() => {
    if (!!homePage && !!filter.startDate && !isReady) {
      setIsReady(true);
    }
  }, [homePage, filter.startDate, isReady]);

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!isReady) return;
    fetchResults(
      generateQuery(
        filterFormat({
          ...settings,
          actions: !!homePage ? [homePage] : [],
          contentTypes: !!contentType ? filter.contentTypes : [],
          startDate: filter.startDate,
          endDate: filter.endDate,
          mediaTypeIds: filter.mediaTypeIds ?? [],
          sourceIds: filter.sourceIds ?? [],
        }),
      ),
    );
    // only run when filter is ready, and when filter.startDate changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isReady, filter.startDate]);

  return (
    <styled.Home>
      <Row>
        <ContentListActionBar
          content={selected}
          onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
        />
      </Row>
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <ContentList
        onContentSelected={handleContentSelected}
        selected={selected}
        content={content}
      />
    </styled.Home>
  );
};
