import { FrontPageGallery } from 'components/front-page-gallery';
import React from 'react';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { IContentModel, IFilterModel, Settings } from 'tno-core';

import * as styled from './styled';

/** simple component used to display front pages on the landing page */
export const FrontPages: React.FC = () => {
  const [, { getFilter }] = useFilters();
  const [, { findContentWithElasticsearch }] = useContent();
  const [{ settings }] = useLookup();

  const [filter, setFilter] = React.useState<IFilterModel>();
  const [content, setContent] = React.useState<IContentModel[]>([]);
  const filterId = settings.find((s) => s.name === Settings.FrontpageFilter)?.value;

  React.useEffect(() => {
    if (filterId && !filter) {
      getFilter(+filterId)
        .then((filter) => setFilter(filter))
        .catch(() => {});
    }
    // Only get the filter if the filterId changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterId]);

  React.useEffect(() => {
    if (filter) {
      findContentWithElasticsearch(filter.query, false, 'frontPage')
        .then((results) => {
          setContent(results.hits?.hits?.map((h) => h._source!) ?? []);
        })
        .catch(() => {});
    }
    // Only get when the filter changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter]);

  return (
    <styled.FrontPages>
      <div className="title">Front Pages</div>
      <FrontPageGallery frontpages={content} />
    </styled.FrontPages>
  );
};
