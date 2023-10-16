import 'react-quill/dist/quill.snow.css';

import { Wysiwyg } from 'components/wysiwyg';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  ContentTypeName,
  FieldSize,
  filterEnabledOptions,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  Show,
  TimeInput,
  useWindowSize,
} from 'tno-core';

import { Topic } from './components';
import { IContentForm } from './interfaces';
import { MediaSummary } from './MediaSummary';
import * as styled from './styled';
import { isSummaryRequired } from './utils';

export interface IContentStoryFormProps {
  contentType: ContentTypeName;
  summaryRequired?: boolean;
}

/**
 * ContentStoryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentStoryForm: React.FC<IContentStoryFormProps> = ({
  contentType,
  summaryRequired: initSummaryRequired,
}) => {
  const [{ series, sources }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const { height } = useWindowSize();

  const [summaryRequired, setSummaryRequired] = React.useState(
    initSummaryRequired ?? isSummaryRequired(values),
  );

  const source = sources.find((s) => s.id === values.sourceId);
  const program = series.find((s) => s.id === values.seriesId);

  const setHours = (e: React.ChangeEvent<HTMLInputElement>) => {
    const date = new Date(values.publishedOn);
    const hours = e.target.value?.split(':');
    if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
      date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
      setFieldValue('publishedOn', moment(date.toISOString()).format('MMM D, yyyy HH:mm:ss'));
    }
  };

  React.useEffect(() => {
    setFieldValue(
      'publishedOnTime',
      !!values.publishedOn ? moment(values.publishedOn).format('HH:mm:ss') : '',
    );
  }, [setFieldValue, values.publishedOn]);

  /** set default value to todays date */
  React.useEffect(() => {
    if (!values.publishedOn) {
      setFieldValue('publishedOn', moment().format('MMM D, yyyy HH:mm:ss'));
    }
  }, [setFieldValue, values.publishedOn]);

  React.useEffect(() => {
    setSeriesOptions(series.map((m: any) => new OptionItem(m.name, m.id, m.isEnabled)));
  }, [series]);

  React.useEffect(() => {
    setSummaryRequired(isSummaryRequired(values));
    // Only interested in changing this value when the product changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.productId]);

  return (
    <styled.ContentStoryForm className="content-properties">
      <Row>
        <Col>
          <Row alignContent="flex-start" alignItems="flex-start">
            <Show visible={contentType !== ContentTypeName.Image}>
              <FormikDatePicker
                name="publishedOn"
                label="Published On"
                required
                autoComplete="false"
                width={FieldSize.Medium}
                selectedDate={
                  !!values.publishedOn ? moment(values.publishedOn).toString() : undefined
                }
                value={!!values.publishedOn ? moment(values.publishedOn).format('MMM D, yyyy') : ''}
                onChange={(date) => {
                  if (!!values.publishedOnTime) {
                    const hours = values.publishedOnTime?.split(':');
                    if (!!hours && !!date) {
                      date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                    }
                  }
                  setFieldValue('publishedOn', moment(date).format('MMM D, yyyy HH:mm:ss'));
                }}
              />
              <TimeInput
                name="publishedOnTime"
                label="Time"
                disabled={!values.publishedOn}
                width="7em"
                value={!!values.publishedOn ? values.publishedOnTime : ''}
                placeholder={!!values.publishedOn ? values.publishedOnTime : 'HH:MM:SS'}
                onBlur={(e) => {
                  if (e.target.value.indexOf('_')) {
                    e.target.value = e.target.value.replaceAll('_', '0');
                    setHours(e);
                  }
                }}
                onChange={(e) => {
                  setHours(e);
                }}
              />
            </Show>
            <Show visible={contentType === ContentTypeName.AudioVideo}>
              <FormikSelect
                name="seriesId"
                label="Show/Program"
                width={FieldSize.Medium}
                value={seriesOptions.find((s: any) => s.value === values.seriesId) ?? ''}
                options={filterEnabledOptions(seriesOptions, values.seriesId)}
                isDisabled={!!values.otherSeries}
                onChange={(e) => {
                  setFieldValue('otherSeries', '');
                }}
              />
              <FormikText
                name="otherSeries"
                label="Other Show/Program"
                width={FieldSize.Medium}
                onChange={(e) => {
                  const value = e.currentTarget.value;
                  setFieldValue('otherSeries', value);
                  if (!!value) setFieldValue('seriesId', undefined);
                }}
                onBlur={() => {
                  const found = series.find(
                    (s) => s.name.toLocaleLowerCase() === values.otherSeries.toLocaleLowerCase(),
                  );
                  if (!!found) {
                    setFieldValue('seriesId', found.id);
                    setFieldValue('otherSeries', '');
                  }
                }}
              />
            </Show>
          </Row>
        </Col>
        <Show
          visible={
            contentType !== ContentTypeName.Image && (source?.useInTopics || program?.useInTopics)
          }
        >
          <Row>
            <div className="vl" />
            <Topic />
          </Row>
        </Show>
      </Row>
      <Show
        visible={
          contentType === ContentTypeName.Image || contentType === ContentTypeName.AudioVideo
        }
      >
        <MediaSummary setShowExpandModal={setShowExpandModal} isSummaryRequired={summaryRequired} />
      </Show>
      <Show
        visible={
          contentType !== ContentTypeName.AudioVideo && contentType !== ContentTypeName.Image
        }
      >
        <Wysiwyg
          className="content-body"
          label="Story"
          fieldName="body"
          expandModal={setShowExpandModal}
        />
      </Show>
      <Modal
        body={
          <Wysiwyg
            className="modal-quill"
            label={
              contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Story
                ? 'Story'
                : 'Summary'
            }
            required={summaryRequired}
            height={height}
            fieldName={
              contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Story
                ? 'body'
                : 'summary'
            }
          />
        }
        isShowing={showExpandModal}
        hide={() => setShowExpandModal(!showExpandModal)}
        className="modal-full"
        customButtons={
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => setShowExpandModal(!showExpandModal)}
          >
            Close
          </Button>
        }
      />
    </styled.ContentStoryForm>
  );
};
