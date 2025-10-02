import { IReportInstanceContentForm } from 'features/my-reports/interfaces';
import { IContentValidationErrors } from 'features/my-reports/interfaces/IContentValidationErrors';
import { getTime } from 'features/my-reports/utils';
import { formatDate } from 'features/utils';
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
  toTitleCase,
  Wysiwyg,
} from 'tno-core';

export interface IUserContentFormProps
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'content'> {
  /** Whether the form is disabled. */
  disabled?: boolean;
  /** The content being edited */
  content?: IReportInstanceContentForm;
  /** Which parts of the form to display */
  show: 'all' | 'summary' | 'none';
  /** Whether content is loading */
  loading?: boolean;
  /** Event fires when content properties are changed. */
  onContentChange?: (content: IReportInstanceContentForm) => void;
  /** Errors on form. */
  errors: IContentValidationErrors | undefined;
}

/**
 * Provides a small form to edit custom content a user owns.
 * @param param0 Component properties.
 * @returns A component.
 */
export const UserContentForm: React.FC<IUserContentFormProps> = ({
  disabled,
  content: values,
  errors,
  show = 'none',
  loading,
  className,
  onContentChange,
  ...rest
}) => {
  const [{ tonePools }] = useLookup();

  const content = values?.content;
  const controlId = React.useCallback(
    (field: string) => `${field}-${content?.id ?? values?.contentId ?? 'new'}`,
    [content?.id, values?.contentId],
  );

  if (!values || !content) return null;

  return show === 'none' ? null : (
    <Col className={`edit-content${className ? ` ${className}` : ''}`} {...rest}>
      <Show visible={loading}>
        <Loading />
      </Show>
      <Show visible={show === 'all'}>
        <TextArea
          id={controlId('headline')}
          name={`headline`}
          label="Headline"
          disabled={disabled}
          rows={1}
          error={errors?.headline}
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
          id={controlId('sourceUrl')}
          name={`sourceUrl`}
          label="URL"
          disabled={disabled}
          value={content.sourceUrl ?? ''}
          onChange={(e) => {
            onContentChange?.({
              ...values,
              content: { ...content, sourceUrl: e.target.value, uid: e.target.value },
            });
          }}
        />
        <Text
          id={controlId('otherSource')}
          name={`otherSource`}
          label="Source"
          disabled={disabled}
          error={errors?.source}
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
          id={controlId('byline')}
          name={`byline`}
          label="Byline"
          disabled={disabled}
          value={content.byline ?? ''}
          onChange={(e) => {
            const value = toTitleCase(e.target.value) ?? '';
            onContentChange?.({
              ...values,
              content: { ...content, byline: value },
            });
          }}
        />
        <Row gap="0.5rem">
          <SelectDate
            id={controlId('publishedOn-date')}
            name={`publishedOn`}
            label="Published On"
            disabled={disabled}
            required
            autoComplete="false"
            error={errors?.publishedOn}
            width={FieldSize.Medium}
            selectedDate={
              !!content.publishedOn ? moment(content.publishedOn).toString() : undefined
            }
            value={!!content.publishedOn ? formatDate(content.publishedOn) : ''}
            onChange={(date) => {
              onContentChange?.({
                ...values,
                content: { ...content, publishedOn: moment(date).toISOString() },
              });
            }}
          />
          <TimeInput
            id={controlId('publishedOn-time')}
            name={`publishedOn`}
            label="Time"
            disabled={!content.publishedOn || disabled}
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
          id={controlId('summary')}
          name={`summary`}
          label="Summary"
          disabled={disabled}
          value={content.summary ?? ''}
          onChange={(text) => {
            onContentChange?.({ ...values, content: { ...content, summary: text } });
          }}
        />
      </Show>
      <Show visible={show === 'all'}>
        <Wysiwyg
          id={controlId('body')}
          name={`body`}
          label="Body"
          disabled={disabled}
          value={content.body ?? ''}
          onChange={(text) => {
            onContentChange?.({ ...values, content: { ...content, body: text } });
          }}
        />
        <SentimentPicker
          id={controlId('tonePools')}
          name="tonePools"
          value={content.tonePools.length ? content.tonePools[0].value : 0}
          disabled={disabled}
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
