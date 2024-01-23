import { PageSection } from 'components/section';
import { Sentiment } from 'components/sentiment';
import { filterFormat, getFilterActions } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup, useNavigateAndScroll } from 'store/hooks';
import {
  ActionName,
  generateQuery,
  IContentModel,
  Row,
} from 'tno-core';

import * as styled from './styled';
import { DetermineContentIcon, isWeekday } from './utils';

export const Commentary: React.FC = () => {
  const [, { findContentWithElasticsearch }] = useContent();
  const [commentary, setCommentary] = React.useState<IContentSearchResult[]>();
  const navigateAndScroll = useNavigateAndScroll();
  const [{ actions }] = useLookup();

  /** determine how far back to grab commentary */
  const determineCommentaryTime = () => {
    const date = new Date();
    if (isWeekday()) {
      date.setHours(date.getHours() - 3);
    } else {
      date.setHours(date.getHours() - 5);
    }
    return moment(date).toISOString();
  };

  React.useEffect(() => {
    if (!!actions && actions.length > 0) {
      let actionFilters = getFilterActions(actions);
      const commentaryAction = actionFilters[ActionName.Commentary];

      findContentWithElasticsearch(
        generateQuery(
          filterFormat({
            actions: [commentaryAction],
            searchUnpublished: false,
            startDate: determineCommentaryTime(),
            size: 100,
          }),
        ),
        false,
      ).then((res) => {
        setCommentary(
          res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          }),
        );
      });
    }
    // only run once
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [actions]);

  return (
    <styled.Commentary>
      <PageSection header="Commentary">
        <div className="content">
          {commentary?.map((x) => {
            return (
              <Row key={x.id} className="content-row">
                <Sentiment value={x.tonePools?.length ? x.tonePools[0].value : 0} />
                <DetermineContentIcon contentType={x.contentType} />
                <div className="headline" onClick={() => navigateAndScroll(`/view/${x.id}`)}>
                  {x.headline}
                </div>
              </Row>
            );
          })}
        </div>
      </PageSection>
    </styled.Commentary>
  );
};
