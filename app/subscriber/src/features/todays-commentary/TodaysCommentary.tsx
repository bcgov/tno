import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat, getFilterActions } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { ActionName, generateQuery, IContentModel } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [
    {
      todaysCommentary: { filter },
    },
    { findContentWithElasticsearch, storeTodaysCommentaryFilter: storeFilter },
  ] = useContent();
  const [{ actions }] = useLookup();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  React.useEffect(() => {
    if (!!actions && actions.length > 0) {
      let actionFilters = getFilterActions(actions);
      const commentaryAction = actionFilters[ActionName.Commentary];

      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            ...filter,
            actions: [commentaryAction],
            startDate: moment(filter.startDate).toISOString(),
            endDate: moment(filter.endDate).toISOString(),
            searchUnpublished: false,
            size: 500,
          }),
        ),
        false,
      )
        .then((res) => {
          setContent(
            res.hits.hits.map((r) => {
              const content = r._source as IContentModel;
              return castToSearchResult(content);
            }),
          );
        })
        .catch();
    }
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [actions, filter]);

  return (
    <styled.TodaysCommentary>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <ContentList
        content={content}
        selected={selected}
        onContentSelected={handleContentSelected}
      />
    </styled.TodaysCommentary>
  );
};
