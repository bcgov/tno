import { DetermineToneIcon } from 'features/home/utils';
import parse from 'html-react-parser';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ContentTypeName, IContentModel, Row, Show, useWindowSize } from 'tno-core';

import * as styled from './styled';
import { formatTime } from './utils';
import { ViewContentToolbar } from './ViewContentToolbar';

export interface IStream {
  url: string;
  type: string;
}

/**
 * Component to display content when navigating to it from the landing page list view, responsive and adaptive to screen size
 * @returns ViewContent component
 */
export const ViewContent: React.FC = () => {
  const { id } = useParams();
  const [content, setContent] = React.useState<IContentModel>();
  const [, { getContent, stream }] = useContent();
  const [avStream, setAVStream] = React.useState<IStream>();
  const { width } = useWindowSize();
  const path = content?.fileReferences ? content?.fileReferences[0]?.path : '';

  const myMinister = localStorage.getItem('myMinister');

  React.useEffect(() => {
    // this will bold the ministers name in the summary or body, only when viewing from the my minister list
    const regex = new RegExp(myMinister ?? '', 'gi');
    if (window.location.href.includes('my-minister')) {
      if (content?.summary && !content.summary.includes(`<b>${myMinister}</b>`))
        setContent({ ...content, summary: content.summary.replace(regex, `<b>${myMinister}</b>`) });

      if (content?.body && !content.body.includes(`<b>${myMinister}</b>`)) {
        setContent({ ...content, body: content.body.replace(regex, `<b>${myMinister}</b>`) });
      }
    }
  }, [content, myMinister]);

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

  const fetchContent = React.useCallback(
    (id: number) => {
      getContent(id).then((content) => {
        if (!!content) {
          setContent(content);
        } else {
        }
      });
    },
    [getContent],
  );

  // add classname for colouring as well as formatting the tone value (+ sign for positive)
  const showToneValue = (tone: number) => {
    if (tone > 0) return <p className="pos">+{tone}</p>;
    if (tone < 0) return <p className="neg">{tone}</p>;
    if (tone === 0) return <p className="neut">{tone}</p>;
  };

  const formatDate = (date: string) => {
    if (date) {
      const d = new Date(date);
      const ye = new Intl.DateTimeFormat('en', { year: 'numeric' }).format(d);
      const mo = new Intl.DateTimeFormat('en', { month: 'short' }).format(d);
      const da = new Intl.DateTimeFormat('en', { day: '2-digit' }).format(d);
      return `${da}-${mo}-${ye} ${formatTime(d)}`;
    } else {
      return '';
    }
  };

  // if statement avoids unwanted fetch when navigating back to home view
  React.useEffect(() => {
    if (window.location.pathname.includes('view')) {
      id && fetchContent(+id);
    }
  }, [id, fetchContent]);

  return (
    <styled.ViewContent>
      <ViewContentToolbar tags={content?.tags ?? []} />
      <Row className="headline-container">
        <p>{content?.headline && content.headline}</p>
        <Row alignItems="center">
          <p className="tone-value">
            {showToneValue(content?.tonePools ? content?.tonePools[0]?.value : 0)}
          </p>
          <DetermineToneIcon tone={content?.tonePools ? content?.tonePools[0]?.value : 0} />
        </Row>
      </Row>
      <Row justifyContent="space-between">
        <p className="name-date">
          {content?.byline ? `by ${content?.byline} - ` : ''}
          {formatDate(content?.publishedOn ?? '')}
        </p>
        <p className="source-section">
          <Show visible={content?.contentType === ContentTypeName.PrintContent}>
            {content?.source?.name} - {content?.section}
          </Show>
          <Show visible={content?.contentType !== ContentTypeName.PrintContent}>
            {!!content?.series && `${content?.source?.name} / ${content?.series?.name}`}
          </Show>
        </p>
      </Row>
      <Show visible={!!avStream && content?.contentType === ContentTypeName.Snippet}>
        <Row justifyContent="center">
          <video
            controls
            height={width! > 500 ? '270' : 135}
            width={width! > 500 ? 480 : 240}
            src={!!avStream?.url ? avStream?.url : ''}
          />
        </Row>
      </Show>
      <Show visible={!!avStream && content?.contentType === ContentTypeName.Image}>
        <Row justifyContent="center">
          <img alt="media" src={!!avStream?.url ? avStream?.url : ''} />
        </Row>
      </Show>
      <Row id="summary" className="summary">
        <Show
          visible={
            content?.contentType === ContentTypeName.PrintContent ||
            content?.contentType === ContentTypeName.Story
          }
        >
          <p>{parse(content?.body ?? '')}</p>
        </Show>
        <Show
          visible={
            content?.contentType === ContentTypeName.Snippet ||
            content?.contentType === ContentTypeName.Image
          }
        >
          <p>{parse(content?.summary ?? '')}</p>
        </Show>
      </Row>
    </styled.ViewContent>
  );
};
