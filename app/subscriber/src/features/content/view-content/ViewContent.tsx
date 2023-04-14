import { DetermineToneIcon } from 'features/home/utils';
import parse from 'html-react-parser';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ContentTypeName, IContentModel, Row, Show } from 'tno-core';

import * as styled from './styled';
import { ViewContentToolbar } from './ViewContentToolbar';

/**
 * Component to display content when navigating to it from the landing page list view, responsive and adaptive to screen size
 * @returns ViewContent component
 */
export const ViewContent: React.FC = () => {
  const { id } = useParams();
  const [content, setContent] = React.useState<IContentModel>();
  const [, { getContent }] = useContent();

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
      return `${da}-${mo}-${ye}`;
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
      <ViewContentToolbar />
      <Row className="headline-container">
        <p>{content?.headline && content.headline}</p>
        <Row alignItems="center">
          <p className="tone-value">
            {showToneValue(content?.tonePools ? content?.tonePools[0].value : 0)}
          </p>
          <DetermineToneIcon tone={content?.tonePools ? content?.tonePools[0].value : 0} />
        </Row>
      </Row>
      <Row justifyContent="space-between">
        <p className="name-date">
          by {content?.owner?.firstName} {content?.owner?.lastName} -{' '}
          {formatDate(content?.publishedOn ?? '')}
        </p>
        <p className="source-section">
          {content?.source?.name} - {content?.section}
        </p>
      </Row>
      <Row id="summary" className="summary">
        <Show visible={content?.contentType === ContentTypeName.Snippet}>
          <p>{parse(content?.summary ?? '')}</p>
        </Show>
        <Show visible={content?.contentType === ContentTypeName.PrintContent}>
          <p>{parse(content?.body ?? '')}</p>
        </Show>
      </Row>
    </styled.ViewContent>
  );
};
