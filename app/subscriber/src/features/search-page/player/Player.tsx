import { IStream } from 'features/content/view-content';
import React from 'react';
import { useContent } from 'store/hooks';
import { Col, IContentModel, Row, Show, useWindowSize } from 'tno-core';

import { stripHtml } from '../utils';
import * as styled from './styled';

export interface IPlayerProps {
  content: IContentModel | null;
  setPlayerOpen: (open: boolean) => void;
}

/** Component used to play media on the search results screen */
export const Player: React.FC<IPlayerProps> = ({ content, setPlayerOpen }) => {
  const [avStream, setAVStream] = React.useState<IStream>();
  const [, { stream }] = useContent();
  const { width } = useWindowSize();
  const fileReference = content?.fileReferences ? content?.fileReferences[0] : undefined;

  React.useEffect(() => {
    if (!!fileReference)
      stream(fileReference.path).then((result) => {
        setAVStream(
          !!result
            ? {
                url: result,
                type: fileReference.contentType,
              }
            : undefined,
        );
      });
    else setAVStream(undefined);
  }, [stream, fileReference]);

  return (
    <styled.Player>
      <Row className="title">
        <p>News Player</p>
        <p onClick={() => setPlayerOpen(false)} className="exit-player">
          X
        </p>
      </Row>
      <div className="body">
        <Row justifyContent="center">
          <Show visible={fileReference?.contentType.startsWith('audio/')}>
            <audio src={avStream?.url} controls>
              HTML5 Audio is required
            </audio>
          </Show>
          <Show visible={fileReference?.contentType.startsWith('video/')}>
            <video
              controls
              height={width! > 500 ? '270' : 135}
              width={width! > 500 ? 480 : 240}
              src={!!avStream?.url ? avStream?.url : ''}
            />
          </Show>
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
