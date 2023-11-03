import { ContentTypeName, IContentModel } from 'tno-core';

export const castToSearchResult = (content: IContentModel) => {
  return {
    id: content.id,
    status: content.status,
    contentType: content.contentType,
    headline: content.headline,
    byline: content.byline,
    edition: content.edition,
    section: content.section,
    page: content.page,
    publishedOn: content.publishedOn,
    source: content.source,
    otherSource: content.otherSource,
    series: content.series,
    product: content.product,
    owner: content.owner,
    isHidden: content.isHidden,
    isApproved: content.isApproved,
    licenseId: content.licenseId,
    tonePools: content.tonePools,
    productId: content.productId,
    summary: content.summary,
    hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
    version: content.version,
  };
};
