import 'react-quill/dist/quill.snow.css';

import { IStream } from 'features/storage/interfaces';
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
  IUserModel,
  Modal,
  OptionItem,
  Row,
  Show,
  TimeInput,
  useKeycloakWrapper,
  useModal,
} from 'tno-core';

import { IFile, Tags, TimeLogSection, ToningGroup, Wysiwyg } from '.';
import { IContentForm } from './interfaces';
import { MediaSummary } from './MediaSummary';
import * as styled from './styled';
import { TimeLogTable } from './TimeLogTable';
import { TopicForm } from './TopicForm';
import { getTotalTime } from './utils';

export interface IContentStoryFormProps {
  setContent: (content: IContentForm) => void;
  content: IContentForm;
  contentType: ContentTypeName;
  savePressed?: boolean;
}

/**
 * ContentStoryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentStoryForm: React.FC<IContentStoryFormProps> = ({
  setContent,
  content,
  contentType,
  savePressed,
}) => {
  const keycloak = useKeycloakWrapper();
  const [{ series, users, sources }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [effort, setEffort] = React.useState(0);

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;
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
  const [stream, setStream] = React.useState<IStream>(); // TODO: Remove dependency coupling with storage component.
  const videoRef = React.useRef<HTMLVideoElement>(null);

  React.useEffect(() => {
    setEffort(getTotalTime(values.timeTrackings));
  }, [values.timeTrackings]);

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
    // TODO: Get MimeType from file.
    setStream(
      !!path ? { url: `/api/editor/contents/stream?path=${path}`, type: 'video/mp4' } : undefined,
    );
  }, [path]);

  React.useEffect(() => {
    if (!!stream && !!videoRef.current) {
      videoRef.current.src = stream.url;
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
            <TopicForm />
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
        />
      </Show>
      <Row>
        <Col flex="1 1 0">
          <Show
            visible={
              contentType !== ContentTypeName.Snippet && contentType !== ContentTypeName.Image
            }
          >
            <Wysiwyg label="Story" fieldName="body" expandModal={setShowExpandModal} />
            <Row>
              <Tags />
              <ToningGroup fieldName="tonePools" />
            </Row>
          </Show>
        </Col>
      </Row>
      <Row className={contentType !== ContentTypeName.Image ? 'multi-section' : ''}>
        <Show visible={contentType === ContentTypeName.Snippet}>
          <Row className="multi-group">
            <TimeLogSection
              toggle={toggle}
              content={content}
              setContent={setContent}
              effort={effort}
              setEffort={setEffort}
              userId={userId!}
            />
            <Modal
              body={
                <Wysiwyg
                  label={contentType === ContentTypeName.PrintContent ? 'Story' : 'Summary'}
                  required
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

            <Modal
              hide={toggle}
              isShowing={isShowing}
              headerText="Prep Time Log"
              body={
                <TimeLogTable
                  setTotalEffort={setEffort}
                  totalEffort={effort}
                  data={values.timeTrackings}
                />
              }
              customButtons={
                <Button variant={ButtonVariant.secondary} onClick={toggle}>
                  Close
                </Button>
              }
            />
          </Row>
        </Show>
      </Row>
    </styled.ContentStoryForm>
  );
};
