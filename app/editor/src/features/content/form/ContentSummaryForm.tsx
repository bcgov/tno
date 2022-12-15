import 'react-quill/dist/quill.snow.css';

import { IOptionItem, OptionItem, RadioGroup, TimeInput, Wysiwyg } from 'components/form';
import { FormikRadioGroup, FormikSelect, FormikText } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal/Modal';
import { IFile, Upload } from 'components/upload';
import { getIn, useFormikContext } from 'formik';
import { useCombinedView } from 'hooks';
import { ContentTypeName, IUserModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import _ from 'lodash';
import moment from 'moment';
import React from 'react';
import { TbLanguage } from 'react-icons/tb';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonVariant, Col, FieldSize, Row, Show, useKeycloakWrapper } from 'tno-core';
import { getSortableOptions } from 'utils';

import { toningOptions } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { TimeLogTable } from './TimeLogTable';
import { getTotalTime } from './utils';

const tagMatch = /\[.*\](?![\S\s])$/g;
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
  const [{ series, categories, licenses, users }] = useLookup();
  const { values, setFieldValue, errors } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [, { download }] = useContent();
  const combined = useCombinedView();

  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const [categoryOptions, setCategoryOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [licenseOptions, setLicenseOptions] = React.useState<IOptionItem[]>([]);
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
  const [streamUrl, setStreamUrl] = React.useState<string>('');
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
    setSeriesOptions(series.map((m: any) => new OptionItem(m.name, m.id)));
  }, [series]);

  React.useEffect(() => {
    setLicenseOptions(licenses.map((m: any) => new OptionItem(m.name, m.id)));
  }, [licenses]);

  React.useEffect(() => {
    setStreamUrl(path ? `/api/editor/contents/upload/stream?path=${path}` : '');
  }, [path]);

  React.useEffect(() => {
    if (!!streamUrl && !!videoRef.current) {
      videoRef.current.src = streamUrl;
    }
  }, [streamUrl, videoRef]);

  const setMedia = () => {
    setStreamUrl(!!streamUrl ? '' : `/api/editor/contents/upload/stream?path=${path}`);
  };

  const toningError = getIn(errors, 'tone');

  return (
    <styled.ContentSummaryForm className="content-properties">
      <Row>
        <Col>
          <Show visible={contentType !== ContentTypeName.Image}>
            <Row>
              <FormikSelect
                name="seriesId"
                label="Series"
                width={FieldSize.Medium}
                value={seriesOptions.find((s: any) => s.value === values.seriesId) ?? ''}
                options={seriesOptions}
                isDisabled={!!values.otherSeries}
                onChange={(e) => {
                  setFieldValue('otherSeries', '');
                }}
              />
              <FormikText
                name="otherSeries"
                label="Other Series"
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
        <div className="vl" />
        <Col className="transcription-section">
          <label className="label">Create Labels</label>
          <Button>
            <TbLanguage className="nlp-button" /> START NLP
          </Button>
        </Col>
        <div className="vl" />
        <Col>
          <FormikSelect
            name="categories"
            label="Event of Day Category"
            width={FieldSize.Medium}
            options={categoryOptions}
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
        {/* <Show visible={contentType !== ContentTypeName.Image}>
          <Col className="licenses">
            <RadioGroup
              name="expireOptions"
              label="Licence"
              spaceUnderRadio
              options={licenseOptions}
              value={licenseOptions.find((e) => e.value === values?.licenseId)}
              onChange={(e) => setFieldValue('licenseId', Number(e.target.value))}
            />
          </Col>
        </Show> */}
      </Row>
      <Show visible={contentType !== ContentTypeName.Image}>
        <Col flex="1 1 0">
          <Show visible={contentType === ContentTypeName.Snippet}>
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
      </Show>
      <Show visible={contentType !== ContentTypeName.Image}>
        <Row className="multi-section">
          <Row className="multi-group">
            <FormikText
              name="tags"
              label="Tags"
              disabled
              width={combined ? FieldSize.Big : FieldSize.Large}
              value={values.tags.map((t) => t.id).join(', ')}
            />
            <Button
              variant={ButtonVariant.danger}
              className="top-spacer"
              onClick={() => {
                setFieldValue('summary', values.summary.replace(tagMatch, ''));
                setFieldValue('tags', []);
              }}
            >
              Clear Tags
            </Button>
          </Row>
          <div className="multi-group">
            <FormikRadioGroup
              label="Toning"
              direction="row"
              className="toning"
              error={savePressed && toningError}
              name="tonePool"
              required
              options={toningOptions}
              onChange={(e, value) => {
                setFieldValue('tonePool', value);
                setFieldValue('tone', value?.value);
              }}
            />
          </div>
          <Show visible={contentType !== ContentTypeName.Image}>
            <Row className="multi-group">
              <FormikText
                width={FieldSize.Small}
                name="prep"
                label="Prep Time (minutes)"
                type="number"
              />
              <Button
                className="top-spacer add-time"
                variant={ButtonVariant.secondary}
                disabled={isNaN((values as any).prep)}
                onClick={() => {
                  setEffort(effort!! + Number((values as any).prep));
                  setFieldValue('timeTrackings', [
                    ...values.timeTrackings,
                    {
                      userId: userId,
                      activity: !!values.id ? 'Updated' : 'Created',
                      effort: (values as any).prep,
                      createdOn: new Date(),
                    },
                  ]);
                  setFieldValue('prep', '');
                }}
              >
                Add
              </Button>
              <FormikText
                disabled
                width={FieldSize.Tiny}
                name="total"
                label="Total"
                value={effort?.toString()}
              />
              <Button
                onClick={() => {
                  setContent({ ...content, timeTrackings: values.timeTrackings });
                  toggle();
                }}
                className="top-spacer"
                variant={ButtonVariant.secondary}
              >
                View Log
              </Button>

              <Modal
                body={<Wysiwyg label="Summary" required fieldName="summary" />}
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
      </Show>
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
              setStreamUrl('');
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
              {!!streamUrl ? 'Hide Player' : 'Show Player'}
            </Button>
          </Show>
        </Row>
        <Show visible={contentType === ContentTypeName.Image && !!streamUrl}>
          <Col>
            <img alt="" className="object-fit" src={streamUrl}></img>
          </Col>
        </Show>
        <Show visible={contentType !== ContentTypeName.Image}>
          <Col className="video" alignItems="stretch">
            <video ref={videoRef} className={!streamUrl ? 'hidden' : ''} controls>
              HTML5 Video is required for this example
            </video>
          </Col>
        </Show>
      </Show>
    </styled.ContentSummaryForm>
  );
};
