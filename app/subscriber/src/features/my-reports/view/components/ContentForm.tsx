import { Bar } from 'components/bar';
import { Sentiment } from 'components/sentiment';
import moment from 'moment';
import React from 'react';
import { useApp } from 'store/hooks';
import { Col, ContentTypeName, IContentModel, Loading, Show, TextArea, Wysiwyg } from 'tno-core';

export interface IContentFormProps extends React.HTMLAttributes<HTMLDivElement> {
  /** The content being edited */
  content?: IContentModel;
  /** Which parts of the form to display */
  show: 'all' | 'summary' | 'none';
  /** Whether content is loading */
  loading?: boolean;
  /** Event fires when content properties are changed. */
  onContentChange?: (content: IContentModel) => void;
}

/**
 * Provides a small form to edit custom content override values for content a user does not own.
 * @param param0 Component properties.
 * @returns A component.
 */
export const ContentForm: React.FC<IContentFormProps> = ({
  content,
  show = 'none',
  loading,
  className,
  onContentChange,
  ...rest
}) => {
  const [{ userInfo }] = useApp();

  if (!content) return null;

  const userId = userInfo?.id ?? 0;
  const headline = content.versions?.[userId]?.headline
    ? content.versions[userId].headline ?? ''
    : content.headline;

  return show === 'none' ? null : (
    <Col className={`edit-content${className ? ` ${className}` : ''}`} {...rest}>
      <Show visible={loading}>
        <Loading />
      </Show>
      <Bar className="content-bar">
        <Col className="byline">by {content.byline}</Col>
        <Col className="date">{moment(content.publishedOn).format('DD-MMM-YYYY hh:mm:ssA')}</Col>
        <Col flex="1"></Col>
        <Col className="source">
          {content.source?.name ?? content.otherSource}
          {content.contentType === ContentTypeName.PrintContent && content.page
            ? ` | ${content.page}`
            : ''}
        </Col>
        <Col className="sentiment">
          <Sentiment
            value={content.tonePools.length ? content.tonePools[0].value : undefined}
            showValue
          />
        </Col>
      </Bar>
      <Show visible={show === 'all'}>
        <TextArea
          name={`headline`}
          label="Headline"
          rows={1}
          onChange={(e) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  headline: e.target.value,
                },
              },
            };
            onContentChange?.(values);
          }}
          value={headline}
        />
      </Show>
      <Show visible={['all', 'summary'].includes(show)}>
        <Wysiwyg
          name={`summary`}
          label="Summary"
          value={content.versions?.[userId]?.summary ?? ''}
          onChange={(text) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  summary: text,
                },
              },
            };
            onContentChange?.(values);
          }}
        />
      </Show>
      <Show visible={show === 'all'}>
        <Wysiwyg
          name={`body`}
          label="Body"
          value={content.versions?.[userId]?.body ?? (content.isApproved ? content.body ?? '' : '')}
          onChange={(text) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  body: text,
                },
              },
            };
            onContentChange?.(values);
          }}
        />
      </Show>
    </Col>
  );
};
