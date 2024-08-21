import React from 'react';
import { useApp } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  IContentModel,
  Loading,
  Show,
  Text,
  TextArea,
  Wysiwyg,
} from 'tno-core';

export interface IContentFormProps extends Omit<React.HTMLAttributes<HTMLDivElement>, 'content'> {
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
  reportContent: { label: string; url: string; section: string }[];
}

/**
 * Provides a small form to edit custom content override values for content a user does not own.
 * @param param0 Component properties.
 * @returns A component.
 */
export const ContentForm: React.FC<IContentFormProps> = ({
  content,
  disabled = false,
  show = 'none',
  loading,
  className,
  reportContent,
  onContentChange,
  ...rest
}) => {
  const [{ userInfo }] = useApp();
  const [{ impersonate }] = useProfileStore();

  if (!content) return null;

  const userId = impersonate?.id ?? userInfo?.id ?? 0;
  const isAV = content.contentType === ContentTypeName.AudioVideo;
  const versions = content.versions?.[userId] ?? {
    byline: content.byline,
    headline: content.headline,
    summary: '',
    body: isAV
      ? content.isApproved && content.body
        ? content.body
        : content.summary
      : content.body,
  };

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
                  ...versions,
                  headline: e.target.value,
                },
              },
            };
            onContentChange?.(values);
          }}
          value={versions.headline}
        />
        <Text
          name={`byline`}
          label="Byline"
          disabled={disabled}
          onChange={(e) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  ...versions,
                  byline: e.target.value,
                },
              },
            };
            onContentChange?.(values);
          }}
          value={versions.byline}
        />
      </Show>
      <Show visible={['all', 'summary'].includes(show)}>
        <Wysiwyg
          name={`summary`}
          label="Summary"
          urlOptions={reportContent}
          value={versions.summary}
          disabled={disabled}
          onChange={(text) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  ...versions,
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
          value={versions.body}
          disabled={disabled}
          urlOptions={reportContent}
          onChange={(text) => {
            const values = {
              ...content,
              versions: {
                ...content.versions,
                [userId]: {
                  ...content.versions?.[userId],
                  ...versions,
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
