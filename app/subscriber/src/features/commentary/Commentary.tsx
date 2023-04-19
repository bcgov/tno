import { ActionName, IContentModel, Row } from 'tno-core';
import * as styled from './styled';
import React from 'react';
import { useContent } from 'store/hooks';
import { DetermineContentIcon, isWeekday } from './utils';
import { DetermineToneIcon } from 'features/home/utils';
import moment from 'moment';

export const Commentary: React.FC = () => {
  const [{}, { findContent }] = useContent();
  const [commentary, setCommentary] = React.useState<IContentModel[]>([]);

  /** determine how far back to grab commentary */
  const determineCommentaryTime = () => {
    const date = new Date();
    if (isWeekday()) {
      date.setHours(date.getHours() - 3);
    } else {
      date.setHours(date.getHours() - 5);
    }
    return date;
  };

  React.useEffect(() => {
    findContent({
      actions: [ActionName.Commentary],
      contentTypes: [],
      publishedStartOn: moment(determineCommentaryTime()).toISOString(),
    }).then((data) => setCommentary(data.items));
  }, []);

  return (
    <styled.Commentary>
      <div className="title">Commentary</div>
      <div className="content">
        {commentary.map((x) => {
          return (
            <Row>
              <DetermineToneIcon tone={x.tonePools?.length ? x.tonePools[0].value : 0} />
              <DetermineContentIcon contentType={x.contentType} />
              <div className="headline">{x.headline}</div>
            </Row>
          );
        })}
      </div>
    </styled.Commentary>
  );
};
