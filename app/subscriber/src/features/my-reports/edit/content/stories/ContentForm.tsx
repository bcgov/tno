import React from 'react';
import { useApp } from 'store/hooks';
import { Col, IContentModel, Loading, Show, TextArea, Wysiwyg } from 'tno-core';

export interface IContentFormProps extends React.HTMLAttributes<HTMLDivElement> {
  /** The content being edited */
  content?: IContentModel;
  /** Whether form is disabled. */
  disabled?: boolean;
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
  disabled,
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
      <Show visible={show === 'all'}>
        <TextArea
          name={`headline`}
          label="Headline"
          rows={1}
          disabled={disabled}
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
          disabled={disabled}
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
          disabled={disabled}
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
