import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { useFilterOptionContext } from 'components/media-type-filters';
import { ContentListActionBar } from 'components/tool-bar';
import { useActionFilters } from 'features/search-page/hooks';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useSettings } from 'store/hooks';
import { generateQuery, IContentModel } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [
    {
      todaysCommentary: { filter },
    },
    { findContentWithElasticsearch, storeTodaysCommentaryFilter: storeFilter },
  ] = useContent();
  const getActionFilters = useActionFilters();
  const { commentaryActionId } = useSettings();
  const { active } = useFilterOptionContext();
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  React.useEffect(() => {
    if (commentaryActionId && !!active) {
      let actionFilters = getActionFilters();
      const commentaryAction = actionFilters.find((a) => a.id === commentaryActionId);

      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            ...filter,
            actions: commentaryAction ? [commentaryAction] : [],
            startDate: filter.startDate ?? moment().startOf('day').toISOString(),
            endDate: filter.endDate ?? moment().endOf('day').toISOString(),
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
  }, [commentaryActionId, filter, findContentWithElasticsearch, getActionFilters, active]);

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
