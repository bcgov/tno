import { Form, Formik } from 'formik';
import React from 'react';
import { useApp, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Col,
  ContentTypeName,
  FormikSentiment,
  IContentModel,
  IContentTonePoolModel,
  Loading,
  Show,
  Text,
  TextArea,
  Wysiwyg,
} from 'tno-core';

import { sentimentFormSchema } from '../../validation/SentimentFormSchema';

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
  const [{ impersonate, myTonePool }] = useProfileStore();
  const [{ tonePools }] = useLookup();
  const userId = impersonate?.id ?? userInfo?.id ?? 0;
  const isAV = content?.contentType === ContentTypeName.AudioVideo;
  const versions = content?.versions?.[userId] ?? {
    byline: content?.byline,
    headline: content?.headline,
    summary: '',
    body: isAV
      ? content.isApproved && content.body
        ? content.body
        : content.summary
      : content?.body,
  };
  const [initialValues, setInitialValues] = React.useState({
    tonePools:
      content?.tonePools && content?.tonePools.length > 0
        ? content.tonePools.filter((pool: IContentTonePoolModel) => pool.ownerId === userId)
        : [
            {
              ...myTonePool,
              value:
                content?.tonePools.find(
                  (pool: { id: number; ownerId: number }) =>
                    pool.id === myTonePool?.id && pool.ownerId === userId,
                )?.value ?? undefined,
            },
          ],
  });
  const handleSentimentChange = React.useCallback(
    async (newTonePool: any) => {
      const validTonePool = Array.isArray(newTonePool) ? newTonePool[0] : null;

      if (!validTonePool) return;

      try {
        if (!content?.id) {
          console.error('Content is missing required properties');
          return;
        }

        const existingTonePoolIndex = content?.tonePools?.findIndex(
          (tonePool) => tonePool.ownerId === userId,
        );
        let updatedContentTonePool;

        if (existingTonePoolIndex !== undefined && existingTonePoolIndex >= 0) {
          // Replace the existing TonePool value
          updatedContentTonePool = content?.tonePools?.map((tonePool, index) =>
            index === existingTonePoolIndex
              ? { ...tonePool, value: validTonePool.value }
              : tonePool,
          );
        } else {
          // Add a new TonePool entry
          updatedContentTonePool = [
            ...(content?.tonePools || []),
            { ...validTonePool, ownerId: userId },
          ];
        }

        const updatedContent: IContentModel = {
          ...content,
          tonePools: updatedContentTonePool as IContentTonePoolModel[],
        };

        onContentChange?.(updatedContent);
      } catch (ex) {
        console.error('Error updating tone pools:', ex);
      }
    },
    [content, onContentChange, userId],
  );

  React.useEffect(() => {
    const updatedTonePools =
      content?.tonePools && content?.tonePools.length > 0
        ? content.tonePools.filter((pool: IContentTonePoolModel) => pool.ownerId === userId)
        : [
            {
              ...myTonePool,
              value:
                content?.tonePools.find(
                  (pool: { id: number; ownerId: number }) =>
                    pool.id === myTonePool?.id && pool.ownerId === userId,
                )?.value ?? undefined,
            },
          ];

    // Update the versions state with the new tonePools
    setInitialValues((prevVersions) => ({
      ...prevVersions,
      tonePools: updatedTonePools,
    }));
  }, [content?.tonePools, userId, myTonePool]);

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
            if (!content || typeof content.id !== 'number') {
              console.error('Content ID is missing or invalid.');
              return;
            }

            const values = {
              ...content,
              versions: {
                ...content?.versions,
                [userId]: {
                  ...content?.versions?.[userId],
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
            if (!content || typeof content.id !== 'number') {
              console.error('Content ID is missing or invalid.');
              return;
            }
            const values = {
              ...content,
              versions: {
                ...content?.versions,
                [userId]: {
                  ...content?.versions?.[userId],
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
            if (!content || typeof content.id !== 'number') {
              console.error('Content ID is missing or invalid.');
              return;
            }
            const values = {
              ...content,
              versions: {
                ...content?.versions,
                [userId]: {
                  ...content?.versions?.[userId],
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
            if (!content || typeof content.id !== 'number') {
              console.error('Content ID is missing or invalid.');
              return;
            }
            const values = {
              ...content,
              versions: {
                ...content?.versions,
                [userId]: {
                  ...content?.versions?.[userId],
                  ...versions,
                  body: text,
                },
              },
            };
            onContentChange?.(values);
          }}
        />
        <Formik
          initialValues={initialValues}
          validationSchema={sentimentFormSchema}
          onSubmit={() => {}}
          enableReinitialize={true}
        >
          {() => (
            <Form>
              <FormikSentiment
                name="tonePools"
                options={[...tonePools, myTonePool]}
                coloredIcon={true}
                defaultTonePoolId={myTonePool.id}
                defaultTonePoolName={myTonePool.name}
                onSentimentChange={handleSentimentChange}
                required
              />
            </Form>
          )}
        </Formik>
      </Show>
    </Col>
  );
};
