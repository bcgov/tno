import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat, getFilterActions } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { ActionName, generateQuery, IContentModel } from 'tno-core';

import * as styled from './styled';

/** Component that displays top stories defaulting to today's date and adjustable via a date filter. */
export const TopStories: React.FC = () => {
  const [
    {
      topStories: { filter },
    },
    { findContentWithElasticsearch, storeTopStoriesFilter: storeFilter },
  ] = useContent();
  const [{ actions }] = useLookup();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!actions.length || !filter.startDate) return;

    let actionFilters = getFilterActions(actions);
    const topStoryAction = actionFilters[ActionName.TopStory];

    findContentWithElasticsearch(
      generateQuery(
        filterFormat({
          ...filter,
          actions: [topStoryAction],
        }),
      ),
      false,
    ).then((res) => {
      setContent(
        res.hits.hits.map((r) => {
          const content = r._source as IContentModel;
          return castToSearchResult(content);
        }),
      );
    });
    // only run this effect when the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, actions]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);
  return (
    <styled.TopStories>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <ContentList
        content={content}
        onContentSelected={handleContentSelected}
        selected={selected}
      />
    </styled.TopStories>
  );
};
