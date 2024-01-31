import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { getTime } from 'features/my-reports/utils';
import moment from 'moment';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Col,
  FieldSize,
  IContentTonePoolModel,
  Loading,
  Row,
  SelectDate,
  SentimentPicker,
  Show,
  Text,
  TextArea,
  TimeInput,
  Wysiwyg,
} from 'tno-core';

export interface IUserContentFormProps extends React.HTMLAttributes<HTMLDivElement> {
  /** The content being edited */
  content?: IReportInstanceContentForm;
  /** Which parts of the form to display */
  show: 'all' | 'summary' | 'none';
  /** Whether content is loading */
  loading?: boolean;
  /** Event fires when content properties are changed. */
  onContentChange?: (content: IReportInstanceContentForm) => void;
}

/**
 * Provides a small form to edit custom content a user owns.
 * @param param0 Component properties.
 * @returns A component.
 */
export const UserContentForm: React.FC<IUserContentFormProps> = ({
  content: values,
  show = 'none',
  loading,
  className,
  onContentChange,
  ...rest
}) => {
  const [{ tonePools }] = useLookup();

  const content = values?.content;

  if (!values || !content) return null;

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
          required
          value={content.headline ?? ''}
          onChange={(e) => {
            onContentChange?.({
              ...values,
              content: { ...content, headline: e.target.value },
            });
          }}
        />
        <Text
          name={`sourceUrl`}
          label="URL"
          value={content.sourceUrl ?? ''}
          onChange={(e) => {
            onContentChange?.({
              ...values,
              content: { ...content, sourceUrl: e.target.value, uid: e.target.value },
            });
          }}
        />
        <Text
          name={`otherSource`}
          label="Source"
          required
          value={content.otherSource ?? ''}
          onChange={(e) => {
            onContentChange?.({
              ...values,
              content: { ...content, otherSource: e.target.value },
            });
          }}
        />
        <Text
          name={`byline`}
          label="Byline"
          value={content.byline ?? ''}
          onChange={(e) => {
            onContentChange?.({
              ...values,
              content: { ...content, byline: e.target.value },
            });
          }}
        />
        <Row gap="0.5rem">
          <SelectDate
            name={`publishedOn`}
            label="Published On"
            required
            autoComplete="false"
            width={FieldSize.Medium}
            selectedDate={
              !!content.publishedOn ? moment(content.publishedOn).toString() : undefined
            }
            value={!!content.publishedOn ? moment(content.publishedOn).format('MMM D, yyyy') : ''}
            onChange={(date) => {
              onContentChange?.({
                ...values,
                content: { ...content, publishedOn: moment(date).toISOString() },
              });
            }}
          />
          <TimeInput
            name={`publishedOn`}
            label="Time"
            disabled={!content.publishedOn}
            width="7em"
            value={!!content.publishedOn ? moment(content.publishedOn).format('HH:mm:ss') : ''}
            placeholder={'HH:MM:SS'}
            onBlur={(e) => {
              if (e.target.value.indexOf('_')) {
                e.target.value = e.target.value.replaceAll('_', '0');
              }

              const date = getTime(e, content.publishedOn);
              if (!!date) {
                onContentChange?.({
                  ...values,
                  content: { ...content, publishedOn: moment(date).toISOString() },
                });
              }
            }}
            onChange={(e) => {
              const date = getTime(e, content.publishedOn);
              if (!!date) {
                onContentChange?.({
                  ...values,
                  content: {
                    ...content,
                    publishedOn: moment(date).format('MMM D, yyyy HH:mm:ss'),
                  },
                });
              }
            }}
          />
        </Row>
      </Show>
      <Show visible={['all', 'summary'].includes(show)}>
        <Wysiwyg
          name={`summary`}
          label="Summary"
          value={content.summary ?? ''}
          onChange={(text) => {
            onContentChange?.({ ...values, content: { ...content, summary: text } });
          }}
        />
      </Show>
      <Show visible={show === 'all'}>
        <Wysiwyg
          name={`body`}
          label="Body"
          value={content.body ?? ''}
          onChange={(text) => {
            onContentChange?.({ ...values, content: { ...content, body: text } });
          }}
        />
        <SentimentPicker
          name="tonePools"
          value={content.tonePools.length ? content.tonePools[0].value : 0}
          onChange={(value: number) => {
            const contentTonePools: IContentTonePoolModel[] = content.tonePools.length
              ? content.tonePools.map<IContentTonePoolModel>((tp, index) =>
                  index === 0 ? { ...tp, value } : tp,
                )
              : [{ ...tonePools[0], value }];
            onContentChange?.({
              ...values,
              content: {
                ...content,
                tonePools: contentTonePools,
              },
            });
          }}
        />
      </Show>
    </Col>
  );
};
