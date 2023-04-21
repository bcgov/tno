import { DetermineToneIcon } from 'features/home/utils';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router';
import { useContent } from 'store/hooks';
import { ActionName, IContentModel, Row } from 'tno-core';

import * as styled from './styled';
import { DetermineContentIcon, isWeekday } from './utils';

export const Commentary: React.FC = () => {
  const [, { findContent }] = useContent();
  const [commentary, setCommentary] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();

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
    findContent({
      actions: [ActionName.Commentary],
      contentTypes: [],
      publishedStartOn: determineCommentaryTime(),
    }).then((data) => setCommentary(data.items));
  }, [findContent]);

  return (
    <styled.Commentary>
      <div className="title">Commentary</div>
      <div className="content">
        {commentary.map((x) => {
          return (
            <Row key={x.id} className="content-row">
              <DetermineToneIcon tone={x.tonePools?.length ? x.tonePools[0].value : 0} />
              <DetermineContentIcon contentType={x.contentType} />
              <div className="headline" onClick={() => navigate(`/view/${x.id}`)}>
                {x.headline}
              </div>
            </Row>
          );
        })}
      </div>
    </styled.Commentary>
  );
};
