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
    postedOn: content.postedOn,
    publishedOn: content.publishedOn,
    source: content.source,
    otherSource: content.otherSource,
    series: content.series,
    mediaType: content.mediaType,
    mediaTypeId: content.mediaTypeId,
    owner: content.owner,
    isHidden: content.isHidden,
    isApproved: content.isApproved,
    isPrivate: content.isPrivate,
    licenseId: content.licenseId,
    tonePools: content.tonePools,
    summary: content.summary,
    hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
    version: content.version,
  };
};
