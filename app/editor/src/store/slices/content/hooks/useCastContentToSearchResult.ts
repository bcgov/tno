import React from 'react';
import { useSettings } from 'store/hooks';
import { ContentTypeName, IContentModel } from 'tno-core';

import { IContentSearchResult } from '../interfaces';

export const useCastContentToSearchResult = () => {
  const { topStoryActionId, commentaryActionId, featuredStoryActionId } = useSettings();

  const castContentToSearchResult = React.useCallback(
    (content: IContentModel): IContentSearchResult => {
      const result: IContentSearchResult = {
        id: content.id,
        original: content,
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
        mediaType: content.mediaType?.name,
        owner: content.owner?.username,
        isHidden: content.isHidden,
        isApproved: content.isApproved,
        hasTranscript: content.contentType === ContentTypeName.AudioVideo && !!content.body,
        isTopStory: topStoryActionId
          ? content.actions.some((a) => a.id === topStoryActionId && a.value === 'true')
          : false,
        isCommentary: commentaryActionId
          ? content.actions.some((a) => a.id === commentaryActionId && a.value !== '')
          : false,
        isFeaturedStory: featuredStoryActionId
          ? content.actions.some((a) => a.id === featuredStoryActionId && a.value === 'true')
          : false,
        version: content.version,
        isCBRAUnqualified: content.isCBRAUnqualified,
      };

      return result;
    },
    [commentaryActionId, featuredStoryActionId, topStoryActionId],
  );

  return castContentToSearchResult;
};
