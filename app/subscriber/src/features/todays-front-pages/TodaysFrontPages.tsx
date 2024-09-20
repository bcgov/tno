import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { FrontPageGallery } from 'components/front-page-gallery';
import React from 'react';
import { useApp, useContent, useSettings } from 'store/hooks';
import { generateFilterQuery, IContentModel, IFilterSettingsModel, Loader } from 'tno-core';

import * as styled from './styled';

/** Component that displays front pages defaulting to today's date and adjustable via a date filter. */
export const TodaysFrontPages: React.FC = () => {
  const [
    {
      frontPage: { filter: frontPageFilter },
    },
    { findContentWithElasticsearch, storeFrontPageFilter: storeFilter },
  ] = useContent();
  const { frontPageImageMediaTypeId } = useSettings(true);
  const [{ requests }] = useApp();

  const [frontPages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [filter, setFilter] = React.useState<IFilterSettingsModel>();

  React.useEffect(() => {
    storeFilter({
      size: 100,
      searchUnpublished: false,
      dateOffset: 0,
      mediaTypeIds: frontPageImageMediaTypeId ? [frontPageImageMediaTypeId] : [],
    });
    // Only update the filter if the frontPageImageMediaTypeId changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [frontPageImageMediaTypeId]);

  const fetchResults = React.useCallback(
    async (query: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(query, false, 'frontPage');
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
    if (
      frontPageFilter &&
      frontPageFilter.mediaTypeIds?.length &&
      (frontPageFilter?.dateOffset !== filter?.dateOffset ||
        frontPageFilter?.startDate !== filter?.startDate ||
        frontPageFilter?.endDate !== filter?.endDate)
    ) {
      fetchResults(generateFilterQuery(frontPageFilter)).catch(() => {});
      setFilter(frontPageFilter);
    }
  }, [fetchResults, filter?.dateOffset, filter?.endDate, filter?.startDate, frontPageFilter]);

  return (
    <styled.TodaysFrontPages>
      <DateFilter filter={frontPageFilter} storeFilter={storeFilter} />
      <Loader visible={requests.some((r) => r.url === 'find-contents-with-elasticsearch')} />
      <FrontPageGallery frontpages={frontPages} />
    </styled.TodaysFrontPages>
  );
};
