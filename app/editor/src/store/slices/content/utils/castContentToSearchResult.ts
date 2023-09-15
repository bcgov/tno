import { ContentTypeName, IContentModel } from 'tno-core';

import { IContentSearchResult } from '../interfaces';

export const castContentToSearchResult = (content: IContentModel): IContentSearchResult => {
  const result: IContentSearchResult = {
    id: content.id,
    status: content.status,
    contentType: content.contentType,
    headline: content.headline,
    byline: content.byline,
    edition: content.edition,
    section: content.section,
    page: content.page,
    publishedOn: content.publishedOn,
    source: content.source?.code,
    otherSource: content.otherSource,
    series: content.series?.name,
    product: content.product?.name,
    owner: content.owner?.username,
    isHidden: content.isHidden,
    isApproved: content.isApproved,
    hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
    version: content.version,
  };

  return result;
};
