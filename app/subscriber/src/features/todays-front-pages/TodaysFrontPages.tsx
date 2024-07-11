import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { FrontPageGallery } from 'components/front-page-gallery';
import React from 'react';
import { useContent, useFilters, useSettings } from 'store/hooks';
import { generateFilterQuery, IContentModel, IFilterModel, IFilterSettingsModel } from 'tno-core';

import { defaultFilter } from './constants';
import * as styled from './styled';

/** Component that displays front pages defaulting to today's date and adjustable via a date filter. */
export const TodaysFrontPages: React.FC = () => {
  const [
    {
      frontPage: { filter: frontPageFilter },
    },
    { findContentWithElasticsearch, storeFrontPageFilter: storeFilter },
  ] = useContent();
  const [, { getFilter }] = useFilters();
  const { frontpageFilterId } = useSettings(true);

  const [frontpages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [selected] = React.useState<IContentModel[]>([]);
  const [filter, setFilter] = React.useState<IFilterModel>({
    ...defaultFilter,
    settings: frontPageFilter,
  });

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false, 'frontPage');
        const mappedResults = res.hits?.hits?.map((h: { _source: IContentModel }) => {
          const content = h._source;
          return {
            id: content.id,
            headline: content.headline,
            section: content.section,
            tonePools: content.tonePools,
            otherSource: content.otherSource,
            source: content.source,
            page: content.page,
            fileReferences: content.fileReferences,
          };
        });
        setFrontPages(mappedResults);
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    if (frontPageFilter.startDate !== filter.settings.startDate) {
      const settings: IFilterSettingsModel = { ...frontPageFilter, dateOffset: undefined };
      setFilter((filter) => ({
        ...filter,
        settings: settings,
        query: generateFilterQuery(settings, filter.query),
      }));
    } else {
      setFilter((filter) => ({ ...filter, settings: frontPageFilter }));
    }
    // Only update the local filter when the front page filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageFilter]);

  React.useEffect(() => {
    if (filter.query.query) {
      fetchResults(filter.query).catch(() => {});
    }
  }, [fetchResults, filter]);

  React.useEffect(() => {
    if (frontpageFilterId && filter.id !== frontpageFilterId) {
      setFilter({ ...defaultFilter, id: frontpageFilterId }); // Do this to stop double fetch.
      getFilter(frontpageFilterId)
        .then((data) => {
          setFilter(data);
        })
        .catch(() => {});
    }
  }, [
    fetchResults,
    filter.id,
    frontPageFilter.endDate,
    frontPageFilter.startDate,
    frontpageFilterId,
    getFilter,
    storeFilter,
  ]);

  return (
    <styled.TodaysFrontPages>
      <DateFilter filter={frontPageFilter} storeFilter={storeFilter} />
      <FrontPageGallery frontpages={frontpages} />
    </styled.TodaysFrontPages>
  );
};
