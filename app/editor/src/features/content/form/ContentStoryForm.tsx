import 'react-quill/dist/quill.snow.css';

import { Wysiwyg } from 'components/wysiwyg';
import { IStream } from 'features/storage/interfaces';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
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
} from 'tno-core';

import { IFile } from '.';
import { Topic } from './components';
import { IContentForm } from './interfaces';
import { MediaSummary } from './MediaSummary';
import * as styled from './styled';

export interface IContentStoryFormProps {
  contentType: ContentTypeName;
  isSummaryRequired: boolean;
}

/**
 * ContentStoryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentStoryForm: React.FC<IContentStoryFormProps> = ({
  contentType,
  isSummaryRequired,
}) => {
  const [{ series, sources }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [, contentApi] = useContent();
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);

  const source = sources.find((s) => s.id === values.sourceId);
  const program = series.find((s) => s.id === values.seriesId);

  const fileReference = values.fileReferences.length ? values.fileReferences[0] : undefined;
  const path = fileReference?.path;
  const file = !!fileReference
    ? ({
        name: fileReference.fileName,
        size: fileReference.size,
      } as IFile)
    : undefined;
  const [race, setRace] = React.useState(false); // Required to handle race condition when initializing the stream.
  // TODO: The stream shouldn't be reset every time the users changes the tab.
  const [stream, setStream] = React.useState<IStream>(); // TODO: Remove dependency coupling with storage component.
  const videoRef = React.useRef<HTMLVideoElement>(null);

  React.useEffect(() => {
    // This whole function is required because React useEffects are designed by 'geniuses' who create wonderful hello world apps.
    // There is a race condition due to no way to wait for state to be updated.
    if (race && !!path) {
      contentApi.stream(path).then((result) => {
        if (race) {
          // TODO: Get MimeType from file.
          const mimeType = 'video/mp4';
          setStream(
            !!result
              ? {
                  url: `data:${mimeType};base64,` + result,
                  type: mimeType,
                }
              : undefined,
          );
          setRace(false);
        }
      });
    }
    // Only interested in executing this when the 'race' begins.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [contentApi, race]);

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
    if (!!path && !stream) {
      setRace(true);
    }
    // We don't want the 'race' dependency to interfere with this logic.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [path, stream]);

  React.useEffect(() => {
    if (!!videoRef.current) {
      if (!!stream) videoRef.current.src = stream.url;
      else videoRef.current.src = '';
    }
  }, [stream, videoRef]);

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
                onChange={(e) => {
                  const date = new Date(values.publishedOn);
                  const hours = e.target.value?.split(':');
                  if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
                    date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                    setFieldValue(
                      'publishedOn',
                      moment(date.toISOString()).format('MMM D, yyyy HH:mm:ss'),
                    );
                  }
                }}
              />
            </Show>
            <Show visible={contentType === ContentTypeName.Snippet}>
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
        visible={contentType === ContentTypeName.Image || contentType === ContentTypeName.Snippet}
      >
        <MediaSummary
          file={file}
          fileReference={fileReference}
          setStream={setStream}
          stream={stream}
          contentType={contentType}
          setShowExpandModal={setShowExpandModal}
          isSummaryRequired={isSummaryRequired}
        />
      </Show>
      <Show
        visible={contentType !== ContentTypeName.Snippet && contentType !== ContentTypeName.Image}
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
            label={contentType === ContentTypeName.PrintContent ? 'Story' : 'Summary'}
            required={isSummaryRequired}
            hasHeight
            fieldName={contentType === ContentTypeName.PrintContent ? 'body' : 'summary'}
          />
        }
        isShowing={showExpandModal}
        hide={() => setShowExpandModal(!showExpandModal)}
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
