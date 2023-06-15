import { IStream } from 'features/content/view-content';
import React from 'react';
import { useContent } from 'store/hooks';
import { Col, IContentModel, Row, useWindowSize } from 'tno-core';

import { stripHtml } from '../utils';
import * as styled from './styled';

export interface IPlayerProps {
  content: IContentModel | null;
}

/** Component used to play media on the search results screen */
export const Player: React.FC<IPlayerProps> = ({ content }) => {
  const [avStream, setAVStream] = React.useState<IStream>();
  const [, { stream }] = useContent();
  const { width } = useWindowSize();
  const path = content?.fileReferences ? content.fileReferences[0]?.path : '';

  React.useEffect(() => {
    if (!!path)
      stream(path).then((result) => {
        const mimeType = 'video/mp4';
        setAVStream(
          !!result
            ? {
                url: `data:${mimeType};base64,` + result,
                type: mimeType,
              }
            : undefined,
        );
      });
    else setAVStream(undefined);
  }, [stream, path]);

  return (
    <styled.Player>
      <div className="title">News Player</div>
      <div className="body">
        <Row justifyContent="center">
          <video
            controls
            height={width! > 500 ? '270' : 135}
            width={width! > 500 ? 480 : 240}
            src={!!avStream?.url ? avStream?.url : ''}
          />
        </Row>
        <Col>
          <p className="source">{content?.source?.name}</p>
          <p className="player-headline">{content?.headline}</p>
          <p className="summary">{stripHtml(content?.summary ?? '')}</p>
        </Col>
      </div>
    </styled.Player>
  );
};
