import 'react-quill/dist/quill.snow.css';

import { Wysiwyg } from 'components/form';
import { Tags } from 'components/form/tags';
import { TimeLogSection } from 'components/form/time-log/TimeLogSection';
import { ToningGroup } from 'components/form/toning/ToningGroup';
import { Modal } from 'components/modal/Modal';
import { IFile, Upload } from 'components/upload';
import { IStream } from 'features/storage/interfaces';
import { useFormikContext } from 'formik';
import { ContentTypeName, IUserModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import _ from 'lodash';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  IOptionItem,
  OptionItem,
  Row,
  Show,
  TimeInput,
  useKeycloakWrapper,
} from 'tno-core';
import { getSortableOptions } from 'utils';

import { IContentForm } from './interfaces';
import * as styled from './styled';
import { TimeLogTable } from './TimeLogTable';
import { getTotalTime } from './utils';

export interface IContentSummaryFormProps {
  setContent: (content: IContentForm) => void;
  content: IContentForm;
  contentType: ContentTypeName;
  savePressed?: boolean;
}

/**
 * ContentSummaryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentSummaryForm: React.FC<IContentSummaryFormProps> = ({
  setContent,
  content,
  contentType,
  savePressed,
}) => {
  const keycloak = useKeycloakWrapper();
  const [{ series, categories, users }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [, { download }] = useContent();

  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const [categoryOptions, setCategoryOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [effort, setEffort] = React.useState(0);

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

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

  // Ensure tag order does not change
  React.useEffect(() => {
    const sortedTags = _.orderBy(values.tags, [(tag) => tag.id.toLowerCase()], ['asc']);
    if (!_.isEqual(sortedTags, values.tags)) {
      setFieldValue('tags', sortedTags);
    }
  }, [setFieldValue, values.tags]);

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
    setCategoryOptions(getSortableOptions(categories));
  }, [categories]);

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

  const setMedia = () => {
    setStream(
      !!stream ? undefined : { url: `/api/editor/contents/stream?path=${path}`, type: 'video/mp4' },
    );
  };

  return (
    <styled.ContentSummaryForm className="content-properties">
      <Row>
        <Col>
          <Show visible={contentType !== ContentTypeName.Image}>
            <Row>
              <FormikSelect
                name="seriesId"
                label="Show/Program"
                width={FieldSize.Medium}
                value={seriesOptions.find((s: any) => s.value === values.seriesId) ?? ''}
                options={filterEnabled(seriesOptions, values.seriesId)}
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
            </Row>
          </Show>
          <Row alignContent="flex-start" alignItems="flex-start">
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
            <Show visible={contentType !== ContentTypeName.Image}>
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
          </Row>
        </Col>
        <Show visible={contentType !== ContentTypeName.Image}>
          <div className="vl" />
          <Col>
            <FormikSelect
              name="categories"
              label="Topics"
              width={FieldSize.Medium}
              options={filterEnabled(
                categoryOptions,
                !!values.categories?.length ? values.categories[0].id : null,
              )}
              value={
                !!values.categories?.length
                  ? categoryOptions.find((c) => c.value === values.categories[0].id)
                  : []
              }
              onChange={(e: any) => {
                // only supports one at a time right now
                let value;
                if (!!e?.value) {
                  value = categories.find((c) => c.id === e.value);
                }
                setFieldValue('categories', !!value ? [value] : []);
              }}
            />
          </Col>
        </Show>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <Show
            visible={
              contentType === ContentTypeName.Snippet || contentType === ContentTypeName.Image
            }
          >
            <Wysiwyg
              label="Summary"
              required
              fieldName="summary"
              expandModal={setShowExpandModal}
            />
          </Show>
          <Show
            visible={
              contentType !== ContentTypeName.Snippet && contentType !== ContentTypeName.Image
            }
          >
            <Wysiwyg label="Story" fieldName="body" expandModal={setShowExpandModal} />
          </Show>
        </Col>
        <Show visible={!!file}>
          <Col className="media" flex="1 1 0">
            <Show visible={contentType === ContentTypeName.Image && !!stream}>
              <Col>
                <img height="360" width="640" alt="" className="object-fit" src={stream?.url}></img>
              </Col>
            </Show>
            <Show visible={contentType !== ContentTypeName.Image}>
              <Col className="video" alignItems="stretch">
                <video
                  height="360"
                  width="640"
                  ref={videoRef}
                  className={!stream ? 'hidden' : ''}
                  controls
                >
                  HTML5 Video is required for this example
                </video>
              </Col>
            </Show>
          </Col>
        </Show>
      </Row>
      <Row className={contentType !== ContentTypeName.Image ? 'multi-section' : ''}>
        <Show visible={contentType !== ContentTypeName.Image}>
          <div className="multi-group">
            <ToningGroup fieldName="tonePools" />
          </div>
        </Show>
        <Tags fieldName="summary" />

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
      <Row className="row-margins"></Row>
      <Show
        visible={contentType === ContentTypeName.Snippet || contentType === ContentTypeName.Image}
      >
        <Row className="row-margins">
          <Upload
            id="upload"
            name="file"
            file={file}
            downloadable={fileReference?.isUploaded}
            onSelect={(e) => {
              const file = !!e.target?.files?.length ? e.target.files[0] : undefined;
              setFieldValue('file', file);
              // Remove file reference.
              setFieldValue('fileReferences', []);
            }}
            onDownload={() => {
              download(values.id, file?.name ?? `${values.otherSource}-${values.id}`);
            }}
            onDelete={() => {
              setStream(undefined);
              if (!!videoRef.current) {
                videoRef.current.src = '';
              }
            }}
          />
          <Show
            visible={(fileReference?.isUploaded && contentType !== ContentTypeName.Image) ?? false}
          >
            <Button
              onClick={() => {
                setMedia();
              }}
              variant={ButtonVariant.secondary}
              className={!file ? 'hidden' : 'show-player'}
            >
              {!!stream ? 'Hide Player' : 'Show Player'}
            </Button>
          </Show>
        </Row>
      </Show>
    </styled.ContentSummaryForm>
  );
};
