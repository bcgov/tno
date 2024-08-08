import { Action } from 'components/action';
import { ContentList } from 'components/content-list';
import { PageSection } from 'components/section';
import { useActionFilters } from 'features/search-page/hooks';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import { noop } from 'lodash';
import React from 'react';
import { FaArrowsSpin } from 'react-icons/fa6';
import { useApiHub, useContent, useSettings } from 'store/hooks';
import {
  generateQuery,
  IContentMessageModel,
  IContentModel,
  MessageTargetKey,
  Row,
} from 'tno-core';

import * as styled from './styled';
import {
  castToSearchResult as castMessageToSearchResult,
  filterCommentaryResults,
  getCutoff,
} from './utils';

/**
 * Provides a section on the page to display the current commentary stories.
 * This displays stories that include the commentary action and are within 3 hours weekdays or 5 on weekends.
 * @returns Component
 */
export const Commentary: React.FC = () => {
  const [, { findContentWithElasticsearch }] = useContent();
  const [commentary, setCommentary] = React.useState<IContentSearchResult[]>([]);
  const { commentaryActionId } = useSettings();
  const getActionFilters = useActionFilters();
  const hub = useApiHub();

  const fetchCommentary = React.useCallback(
    async (commentaryActionId: number) => {
      let actionFilters = getActionFilters();
      const commentaryAction = actionFilters.find((a) => a.id === commentaryActionId);

      const query = generateQuery(
        filterFormat({
          actions: commentaryAction ? [commentaryAction] : [],
          searchUnpublished: false,
          startPostedDate: getCutoff(),
          size: 100,
        }),
      );

      const res = await findContentWithElasticsearch(query, false);
      setCommentary(
        filterCommentaryResults(
          res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          }),
          commentaryActionId,
        ),
      );
    },
    [findContentWithElasticsearch, getActionFilters],
  );

  React.useEffect(() => {
    if (commentaryActionId) {
      fetchCommentary(commentaryActionId).catch(() => {});
    }
  }, [commentaryActionId, fetchCommentary]);

  const onContentReceived = React.useCallback(
    async (message: IContentMessageModel) => {
      const isCommentary = message.actions.some((a) => a.id === commentaryActionId);
      if (isCommentary) {
        setCommentary((commentary) => {
          const content = castMessageToSearchResult(message);
          let found = false;
          const result = commentary.map((c) => {
            if (c.id === content.id) {
              found = true;
              return content;
            }
            return c;
          });

          return filterCommentaryResults(
            found ? result : [content, ...commentary],
            commentaryActionId ?? 0,
          );
        });
      }
    },
    [commentaryActionId],
  );
  hub.useHubEffect(MessageTargetKey.ContentUpdated, onContentReceived);
  hub.useHubEffect(MessageTargetKey.ContentAdded, onContentReceived);

  return (
    <styled.Commentary className="commentary-box">
      <PageSection
        header={
          <Row justifyContent="space-between" flex="1">
            Commentary
            <Action
              title="Refresh"
              icon={<FaArrowsSpin />}
              onClick={() => commentaryActionId && fetchCommentary(commentaryActionId)}
            />
          </Row>
        }
      >
        <ContentList selected={[]} onContentSelected={noop} content={commentary} simpleView />
      </PageSection>
    </styled.Commentary>
  );
};
