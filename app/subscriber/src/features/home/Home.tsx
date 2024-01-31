import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { IGroupByState, IToggleStates } from 'components/content-list/interfaces';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { createFilterSettings } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React, { useMemo } from 'react';
import { useContent, useLookup } from 'store/hooks';
import { generateQuery, IContentModel, IFilterSettingsModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IHomeProps {
  contentViewOptions: IToggleStates;
  groupBy: IGroupByState;
}
/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC<IHomeProps> = ({ contentViewOptions, groupBy }) => {
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch, storeHomeFilter: storeFilter, stream },
  ] = useContent();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
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

  const createStream = async (item: IContentSearchResult) => {
    const fileReference = item?.fileReferences ? item?.fileReferences[0] : undefined;
    if (!!fileReference) return stream(fileReference.path);
    return undefined;
  };

  const sortFunc = (key: string) => {
    switch (key) {
      case 'published':
        return (a: IContentModel, b: IContentModel) => (a.publishedOn > b.publishedOn ? 1 : -1);
      case 'source':
        return (a: IContentModel, b: IContentModel) => {
          if (a.source && b.source) {
            return a.source.sortOrder > b.source.sortOrder ? 1 : -1;
          }
          return -1;
        };
      default:
        return (a: IContentModel, b: IContentModel) => (a.publishedOn > b.publishedOn ? 1 : -1);
    }
  };

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        let firstSort = 'published';
        let secondSort = 'source';
        const res: any = await findContentWithElasticsearch(filter, false);
        setContent(
          res.hits.hits
            .map((h: { _source: IContentModel }) => h._source)
            .sort(sortFunc(firstSort))
            .sort(sortFunc(secondSort)),
        );
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  const displayMedia = async (r: IContentSearchResult) => {
    const list = [...content];
    const e = list.find((e) => e.id === r.id);
    if (!!e) {
      if (!e.mediaUrl) {
        createStream(e).then((result) => {
          e.mediaUrl = result;
          e.displayMedia = result !== undefined ? true : false;
        });
      } else {
        e.displayMedia = !e.displayMedia;
      }
      setContent(list);
    }
  };

  React.useEffect(() => {
    // stops invalid requests before filter is synced with date
    if (!filter.startDate) return;
    fetchResults(
      generateQuery(
        filterFormat({
          ...settings,
          contentTypes: !!contentType ? filter.contentTypes : [],
          featured: true,
          startDate: filter.startDate,
          endDate: filter.endDate,
          mediaTypeIds: filter.mediaTypeIds ?? [],
          sourceIds: filter.sourceIds ?? [],
        }),
      ),
    );
  }, [fetchResults, filter, settings, contentType, actions]);

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
        setSelected={setSelected}
        selected={selected}
        toggleStates={contentViewOptions}
        groupBy={groupBy}
        content={content}
      />
    </styled.Home>
  );
};
