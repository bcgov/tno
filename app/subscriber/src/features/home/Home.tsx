import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { useFilterOptionContext } from 'components/media-type-filters';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { createFilterSettings, getBooleanActionValue } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel, Row } from 'tno-core';

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

  const { hasProcessedInitialPreferences } = useFilterOptionContext();
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const { featuredStoryActionId } = useSettings(true);

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

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    // wait for userinfo incase applying previously viewed filter
    if (!!featuredStoryActionId && hasProcessedInitialPreferences) {
      fetchResults(
        generateQuery(
          filterFormat({
            ...createFilterSettings(
              filter.startDate ?? moment().startOf('day').toISOString(),
              filter.endDate ?? moment().endOf('day').toISOString(),
            ),
            actions: [getBooleanActionValue(featuredStoryActionId)],
            contentTypes: filter.contentTypes ?? [],
            mediaTypeIds: filter.mediaTypeIds ?? [],
            sourceIds: filter.sourceIds ?? [],
          }),
        ),
      );
    }
  }, [filter, fetchResults, featuredStoryActionId, hasProcessedInitialPreferences]);

  return (
    <styled.Home>
      <Row>
        <ContentListActionBar
          content={selected}
          onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
          onClear={() => setSelected([])}
        />
      </Row>
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <ContentList
        onContentSelected={handleContentSelected}
        showDate
        selected={selected}
        content={content}
      />
    </styled.Home>
  );
};
