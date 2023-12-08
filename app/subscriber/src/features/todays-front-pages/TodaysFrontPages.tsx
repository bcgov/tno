import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { FrontPageGallery } from 'components/front-page-gallery';
import moment from 'moment';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { IContentModel, IFilterModel, Settings } from 'tno-core';

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
  const [frontpages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [selected] = React.useState<IContentModel[]>([]);
  const [{ settings }] = useLookup();
  const [, { getFilter }] = useFilters();
  const [filter, setFilter] = React.useState<IFilterModel>(defaultFilter);

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
    if (!!filter.query.query) {
      const calendarStartDate = moment(frontPageFilter.startDate).toISOString();
      const filterStartDate = filter.query.query.bool.must[0].range.publishedOn.gte;
      if (calendarStartDate !== filterStartDate) {
        const range = {
          range: {
            publishedOn: {
              gte: moment(frontPageFilter.startDate).toISOString(),
              lte: moment(frontPageFilter.endDate).toISOString(),
              time_zone: 'US/Pacific',
            },
          },
        };
        const newFilter = { ...filter };
        newFilter.query.query.bool.must[0] = range;
        setFilter(newFilter);
        fetchResults(newFilter.query);
      }
    }
  }, [fetchResults, filter, frontPageFilter]);

  React.useEffect(() => {
    const range = {
      range: {
        publishedOn: {
          gte: moment(frontPageFilter.startDate).toISOString(),
          lte: moment(frontPageFilter.endDate).toISOString(),
          time_zone: 'US/Pacific',
        },
      },
    };
    const filterId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;
    if (filterId) {
      const id: number = parseInt(filterId);
      if (filter?.id !== id) {
        setFilter({ ...defaultFilter, id }); // Do this to stop double fetch.
        getFilter(id).then((data) => {
          data.query.query.bool.must[0] = range;
          setFilter(data);
          fetchResults(data.query);
        });
      }
    } else if (!!settings.length) {
      toast.error(`${Settings.FrontpageFilter} setting needs to be configured.`);
    }
  }, [fetchResults, filter?.id, getFilter, settings, frontPageFilter, filter]);

  return (
    <styled.TodaysFrontPages>
      <FolderSubMenu selectedContent={selected} />
      <DateFilter filter={frontPageFilter} storeFilter={storeFilter} />
      <FrontPageGallery frontpages={frontpages} />
    </styled.TodaysFrontPages>
  );
};
