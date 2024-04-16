import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { createFilterSettings, getBooleanActionValue } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { generateQuery, IContentModel, Row, Settings } from 'tno-core';

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

  const [{ settings: appSettings }] = useLookup();
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

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

  const featuredItemId = React.useMemo(() => {
    const value = appSettings?.find((s) => s.name === Settings.FeaturedAction)?.value;
    if (!value && !!appSettings?.length)
      toast.error(
        'No FeaturedActionId found in settings. Please contact your administrator to update.',
      );
    return value;
  }, [appSettings]);

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!!featuredItemId && !!filter.startDate) {
      fetchResults(
        generateQuery(
          filterFormat({
            ...createFilterSettings(
              filter.startDate ?? moment().startOf('day').toISOString(),
              filter.endDate ?? moment().endOf('day').toISOString(),
            ),
            actions: [getBooleanActionValue(featuredItemId)],
            contentTypes: filter.contentTypes ?? [],
            mediaTypeIds: filter.mediaTypeIds ?? [],
            sourceIds: filter.sourceIds ?? [],
          }),
        ),
      );
    }
  }, [filter, fetchResults, featuredItemId]);

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
        selected={selected}
        content={content}
      />
    </styled.Home>
  );
};
