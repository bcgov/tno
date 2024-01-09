import { FormikForm } from 'components/formik';
import { FormikProps } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaBars, FaCopy, FaExternalLinkAlt } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useLookup, useLookupOptions } from 'store/hooks';
import { IAjaxRequest } from 'store/slices';
import {
  Area,
  Button,
  ButtonVariant,
  Checkbox,
  Claim,
  Col,
  ContentStatusName,
  ContentTypeName,
  FieldSize,
  filterEnabledOptions,
  FormikCheckbox,
  FormikDatePicker,
  FormikHidden,
  FormikSelect,
  FormikSentiment,
  FormikText,
  FormikTextArea,
  FormPage,
  hasErrors,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  Show,
  Tab,
  Tabs,
  TimeInput,
  useModal,
  useTabValidationToasts,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { isWorkOrderStatus } from '../utils';
import { ContentFormSchema } from '../validation';
import {
  ContentClipForm,
  ContentLabelsForm,
  ContentNavigation,
  ContentStoryForm,
  ContentTranscriptForm,
} from '.';
import { ContentFormToolBar, IFile, Tags, TimeLogSection, Topic, Upload } from './components';
import { useContentForm } from './hooks';
import { ImageSection } from './ImageSection';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { toModel } from './utils';
import { WorkOrderStatus } from './WorkOrderStatus';

export interface IContentFormProps {
  /** Control what form elements are visible. */
  contentType?: ContentTypeName;
  /** Whether to scroll to content when loaded. */
  scrollToContent?: boolean;
  /** Root path for combined view. */
  combinedPath?: string;
}

/**
 * Content Form edit and create form for default view. Path will be appended with content id.
 * @param param0 Component properties.
 * @returns Edit/Create Form for Content
 */
const ContentForm: React.FC<IContentFormProps> = ({
  contentType: initContentType = ContentTypeName.AudioVideo,
  combinedPath,
}) => {
  const navigate = useNavigate();
  const {
    userInfo,
    form,
    setForm,
    fetchContent,
    deleteContent,
    handleSave,
    handlePublish,
    handleUnpublish,
    handleTranscribe,
    handleNLP,
    goToNext,
    file,
    fileReference,
    stream,
    setStream,
    download,
  } = useContentForm({
    contentType: initContentType,
    combinedPath,
  });
  const [{ contributorOptions, sources, series, sourceOptions, mediaTypeOptions }] =
    useLookupOptions();
  const [{ tonePools }] = useLookup();
  const { setShowValidationToast } = useTabValidationToasts();

  const { isShowing: showDeleteModal, toggle: toggleDelete } = useModal();
  const { isShowing: showTranscribeModal, toggle: toggleTranscribe } = useModal();
  const { isShowing: showNLPModal, toggle: toggleNLP } = useModal();

  const [size, setSize] = React.useState(1); // TODO: change this to use css media types instead.
  const [active, setActive] = React.useState('summary');
  const [allowPublishWithoutFile, setAllowPublishWithoutFile] = React.useState(false);
  const [, setClipErrors] = React.useState<string>('');
  const [textDecorationStyle, setTextDecorationStyle] = React.useState('none');
  const [cursorStyle, setCursorStyle] = React.useState('text');
  const [savePressed, setSavePressed] = React.useState(false);
  const [isEditing, setIsEditing] = React.useState(false);
  const [parsedTags, setParsedTags] = React.useState<string[]>([]);

  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);

  const source = sources.find((s) => s.id === form.sourceId);
  const program = series.find((s) => s.id === form.seriesId);

  const getHours = (e: React.ChangeEvent<HTMLInputElement>) => {
    const date = new Date(form.publishedOn);
    const hours = e.target.value?.split(':');
    if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
      date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
      return date;
    }
    return null;
  };

  const parseTags = (x: string[]) => {
    setParsedTags(x);
  };

  React.useEffect(() => {
    setForm({
      ...form,
      publishedOnTime: !!form.publishedOn ? moment(form.publishedOn).format('HH:mm:ss') : '',
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.publishedOn, setForm]);

  /** set default value to todays date */
  React.useEffect(() => {
    if (!form.publishedOn) {
      setForm({ ...form, publishedOn: moment().format('MMM D, yyyy HH:mm:ss') });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.publishedOn, setForm]);

  React.useEffect(() => {
    setSeriesOptions(series.map((m: any) => new OptionItem(m.name, m.id, !m.isEnabled)));
  }, [series]);

  React.useEffect(() => {
    if (form.id) setIsEditing(true);
  }, [form.id]);

  return (
    <styled.ContentForm className="content-form fvh">
      <FormPage className="fvh">
        <Area className="area fvh">
          <FormikForm
            className="fvh"
            onSubmit={handlePublish}
            validationSchema={ContentFormSchema}
            validateOnChange={false}
            initialValues={form}
            loading={(request: IAjaxRequest) =>
              !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
            }
          >
            {(props: FormikProps<IContentForm>) => (
              <Col className="content-col fvh">
                <ContentFormToolBar fetchContent={fetchContent} combinedPath={combinedPath} />
                <FormikHidden name="uid" />
                <Row alignItems="flex-start" className="content-details fvh">
                  <Show visible={size === 0}>
                    <Row flex="1 1 100%" wrap="nowrap">
                      <Col flex="1 1 0%">
                        <FormikTextArea
                          name="headline"
                          required
                          label="Headline"
                          value={props.values.headline}
                        />
                      </Col>
                      <Col>
                        <Button
                          className="minimize-details"
                          variant={ButtonVariant.link}
                          tooltip="Maximize Details"
                          onClick={() => setSize(1)}
                        >
                          <FaBars />
                        </Button>
                      </Col>
                    </Row>
                  </Show>
                  <Show visible={size === 1}>
                    <Col flex="1.5 1 0%">
                      <Row wrap="nowrap">
                        <Col flex="1 1 0%">
                          <FormikTextArea
                            name="headline"
                            required
                            className="headline"
                            label="Headline"
                            value={props.values.headline}
                          />
                        </Col>
                        <Col>
                          {/* AudioVideo form */}
                          <Show visible={props.values.contentType !== ContentTypeName.Image}>
                            <Row>
                              <FormikSelect
                                name="sourceId"
                                label="Source"
                                width={FieldSize.Big}
                                value={
                                  sourceOptions.find((mt) => mt.value === props.values.sourceId) ??
                                  ''
                                }
                                onChange={(newValue: any) => {
                                  if (!!newValue) {
                                    const source = sources.find((ds) => ds.id === newValue.value);
                                    props.setFieldValue('sourceId', newValue.value);
                                    props.setFieldValue('otherSource', source?.code ?? '');
                                    if (!!source?.licenseId)
                                      props.setFieldValue('licenseId', source.licenseId);
                                    if (!!source?.mediaTypeId)
                                      props.setFieldValue('mediaTypeId', source.mediaTypeId);
                                  }
                                }}
                                options={filterEnabledOptions(
                                  sourceOptions,
                                  props.values.sourceId,
                                ).filter(
                                  (x) =>
                                    props.values.contentType !== ContentTypeName.Image ||
                                    (typeof x.label === 'string'
                                      ? (x.label as string).includes('(TC)') ||
                                        (x.label as string).includes('(PROVINCE)') ||
                                        (x.label as string).includes('(GLOBE)') ||
                                        (x.label as string).includes('(POST)') ||
                                        (x.label as string).includes('(SUN)')
                                      : false),
                                )}
                                required={
                                  !props.values.otherSource || props.values.otherSource !== ''
                                }
                                isDisabled={!!props.values.tempSource}
                              />
                              <FormikSelect
                                name="mediaTypeId"
                                value={
                                  mediaTypeOptions.find(
                                    (mt) => mt.value === props.values.mediaTypeId,
                                  ) ?? ''
                                }
                                label="Media Type"
                                width={FieldSize.Small}
                                options={mediaTypeOptions}
                                required
                              />
                              <FormikHidden name="otherSource" />
                              <FormikText
                                name="tempSource"
                                label="Other Source"
                                onChange={(e) => {
                                  const value = e.currentTarget.value;
                                  props.setFieldValue('tempSource', value);
                                  props.setFieldValue('otherSource', value);
                                  if (!!value) {
                                    props.setFieldValue('sourceId', undefined);
                                  }
                                }}
                                required={!!props.values.tempSource}
                              />
                            </Row>
                          </Show>
                        </Col>
                      </Row>
                      {/* Image form layout */}
                      <Show visible={props.values.contentType === ContentTypeName.Image}>
                        <ImageSection />
                      </Show>
                      <Show visible={props.values.contentType === ContentTypeName.PrintContent}>
                        <Row>
                          <FormikText name="byline" label="Byline" required={!isEditing} />
                          <FormikSelect
                            name="contributorId"
                            value={
                              contributorOptions.find(
                                (mt) => mt.value === props.values.contributorId,
                              ) ?? ''
                            }
                            label="Columnist/Pundit"
                            width={FieldSize.Small}
                            options={contributorOptions}
                          />
                          <FormikText name="edition" label="Edition" />
                          <FormikText name="section" label="Section" required={!isEditing} />
                          <FormikText name="page" label="Page" />
                        </Row>
                      </Show>
                      <Show
                        visible={
                          props.values.contentType === ContentTypeName.AudioVideo ||
                          props.values.contentType === ContentTypeName.Internet
                        }
                      >
                        <Row>
                          <FormikText name="byline" label="Byline" />
                          <FormikSelect
                            name="contributorId"
                            value={
                              contributorOptions.find(
                                (mt) => mt.value === props.values.contributorId,
                              ) ?? ''
                            }
                            label="Columnist/Pundit"
                            width={FieldSize.Small}
                            options={contributorOptions}
                          />
                          <Col flex="1">
                            <FormikText
                              style={{ textDecoration: textDecorationStyle, cursor: cursorStyle }}
                              className="source-url"
                              name="sourceUrl"
                              label="Source URL"
                              tooltip="The URL to the original source story"
                              onKeyDown={(e) => {
                                if (e.ctrlKey && props.values.sourceUrl) {
                                  setTextDecorationStyle('underline');
                                  setCursorStyle('pointer');
                                }
                              }}
                              onKeyUp={() => {
                                if (textDecorationStyle !== 'none') setTextDecorationStyle('none');
                                if (cursorStyle !== 'text') setCursorStyle('text');
                              }}
                              onClick={(e) => {
                                if (e.ctrlKey && props.values.sourceUrl) {
                                  window.open(props.values.sourceUrl, '_blank', 'noreferrer');
                                }
                              }}
                            >
                              <FaCopy
                                className="icon-button src-cpy"
                                onClick={() => {
                                  if (props.values.sourceUrl) {
                                    navigator.clipboard.writeText(props.values.sourceUrl);
                                  }
                                }}
                              />
                              <FaExternalLinkAlt
                                className={`icon-button ${!props.values.sourceUrl && 'disabled'}`}
                                onClick={() => {
                                  if (props.values.sourceUrl) {
                                    window.open(props.values.sourceUrl, '_blank', 'noreferrer');
                                  }
                                }}
                              />
                            </FormikText>
                          </Col>
                        </Row>
                      </Show>
                      <Row>
                        <Col>
                          <Row alignContent="flex-start" alignItems="flex-start">
                            <Show visible={props.values.contentType !== ContentTypeName.Image}>
                              <FormikDatePicker
                                name="publishedOn"
                                label="Published On"
                                required
                                autoComplete="false"
                                width={FieldSize.Medium}
                                selectedDate={
                                  !!props.values.publishedOn
                                    ? moment(props.values.publishedOn).toString()
                                    : undefined
                                }
                                value={
                                  !!props.values.publishedOn
                                    ? moment(props.values.publishedOn).format('MMM D, yyyy')
                                    : ''
                                }
                                onChange={(date) => {
                                  if (!!props.values.publishedOnTime) {
                                    const hours = props.values.publishedOnTime?.split(':');
                                    if (!!hours && !!date) {
                                      date.setHours(
                                        Number(hours[0]),
                                        Number(hours[1]),
                                        Number(hours[2]),
                                      );
                                    }
                                  }
                                  props.setFieldValue(
                                    'publishedOn',
                                    moment(date).format('MMM D, yyyy HH:mm:ss'),
                                  );
                                }}
                              />
                              <TimeInput
                                name="publishedOnTime"
                                label="Time"
                                disabled={!props.values.publishedOn}
                                width="7em"
                                value={
                                  !!props.values.publishedOn ? props.values.publishedOnTime : ''
                                }
                                placeholder={
                                  !!props.values.publishedOn
                                    ? props.values.publishedOnTime
                                    : 'HH:MM:SS'
                                }
                                onBlur={(e) => {
                                  if (e.target.value.indexOf('_')) {
                                    e.target.value = e.target.value.replaceAll('_', '0');
                                  }
                                  const date = getHours(e);
                                  if (!!date) {
                                    props.setFieldValue(
                                      'publishedOnTime',
                                      moment(date).format('HH:mm:ss'),
                                    );
                                  }
                                }}
                                onChange={(e) => {
                                  const date = getHours(e);
                                  if (!!date) {
                                    props.setFieldValue(
                                      'publishedOnTime',
                                      moment(date).format('HH:mm:ss'),
                                    );
                                  }
                                }}
                              />
                            </Show>
                            <Show visible={props.values.contentType === ContentTypeName.AudioVideo}>
                              <Row nowrap>
                                <FormikSelect
                                  name="seriesId"
                                  label="Show/Program"
                                  width={FieldSize.Medium}
                                  value={
                                    seriesOptions.find(
                                      (s: any) => s.value === props.values.seriesId,
                                    ) ?? ''
                                  }
                                  options={filterEnabledOptions(
                                    seriesOptions,
                                    props.values.seriesId,
                                  )}
                                  isDisabled={!!props.values.otherSeries}
                                  onChange={(e) => {
                                    props.setFieldValue('otherSeries', '');
                                  }}
                                />
                                <FormikText
                                  name="otherSeries"
                                  label="Other Show/Program"
                                  width={FieldSize.Medium}
                                  onChange={(e) => {
                                    const value = e.currentTarget.value;
                                    props.setFieldValue('otherSeries', value);
                                    if (!!value) props.setFieldValue('seriesId', undefined);
                                  }}
                                  onBlur={() => {
                                    const found = series.find(
                                      (s) =>
                                        s.name.toLocaleLowerCase() ===
                                        props.values.otherSeries.toLocaleLowerCase(),
                                    );
                                    if (!!found) {
                                      props.setFieldValue('seriesId', found.id);
                                      props.setFieldValue('otherSeries', '');
                                    }
                                  }}
                                />
                              </Row>
                            </Show>
                          </Row>
                        </Col>
                        <Show
                          visible={
                            props.values.contentType !== ContentTypeName.Image &&
                            (source?.useInTopics || program?.useInTopics)
                          }
                        >
                          <Row>
                            <div className="vl" />
                            <Topic />
                          </Row>
                        </Show>
                      </Row>
                    </Col>
                  </Show>
                </Row>
                <Row flex="1" wrap="nowrap" gap="0.5rem" className="section-upload">
                  <Show visible={props.values.contentType === ContentTypeName.Image}>
                    <ContentStoryForm
                      contentType={ContentTypeName.Image}
                      checkTags={(c: string[]) => parseTags(c)}
                    />
                    <Col flex="1 1 0%" justifyContent="center">
                      <Upload
                        className="media"
                        contentType={form.contentType}
                        id="upload"
                        name="file"
                        file={file}
                        stream={stream}
                        downloadable={fileReference?.isUploaded}
                        onSelect={(e) => {
                          const file = (e as IFile).name ? (e as IFile) : undefined;
                          props.setFieldValue('file', file);
                          // Remove file reference.
                          props.setFieldValue('fileReferences', []);
                        }}
                        onDownload={() => {
                          download(
                            props.values.id,
                            file?.name ?? `${props.values.otherSource}-${props.values.id}`,
                          );
                        }}
                        onDelete={() => {
                          setStream(undefined);
                        }}
                      />
                    </Col>
                  </Show>
                </Row>
                <Row className="tab-section fvh">
                  <Show
                    visible={
                      props.values.contentType !== ContentTypeName.PrintContent &&
                      props.values.contentType !== ContentTypeName.Image
                    }
                  >
                    <Tabs
                      className={`fvh ${size === 1 ? 'small' : 'large'}`}
                      tabs={
                        <>
                          <Tab
                            label="Summary"
                            onClick={() => {
                              setActive('summary');
                            }}
                            active={active === 'summary'}
                            hasErrors={
                              hasErrors(props.errors, ['publishedOn', 'tone', 'summary']) &&
                              active !== 'summary'
                            }
                            showErrorOnSave={{ value: true, savePressed: savePressed }}
                            setShowValidationToast={setShowValidationToast}
                          />
                          <Show visible={props.values.contentType === ContentTypeName.AudioVideo}>
                            <Tab
                              onClick={() => setActive('transcript')}
                              active={active === 'transcript'}
                            >
                              <Row alignItems="center" gap="0.25rem">
                                <WorkOrderStatus
                                  workOrders={form.workOrders}
                                  type={WorkOrderTypeName.Transcription}
                                />
                                <span>Transcript</span>
                                <Show visible={!!props.values.body}>
                                  <FormikCheckbox
                                    name="isApproved"
                                    label="Approved"
                                    className="approve-transcript"
                                  />
                                </Show>
                              </Row>
                            </Tab>
                            {/* <Tab
                              label="Clips"
                              onClick={() => setActive('clips')}
                              active={active === 'clips'}
                              hasErrors={!!clipErrors && active !== 'clips'}
                              showErrorOnSave={{ value: true, savePressed: savePressed }}
                            /> */}
                          </Show>
                          {/* <Show
                            visible={
                              props.values.contentType !== ContentTypeName.Image &&
                              props.values.contentType !== ContentTypeName.AudioVideo
                            }
                          >
                            <Tab
                              label="Labels"
                              onClick={() => setActive('labels')}
                              active={active === 'labels'}
                            >
                              <WorkOrderStatus
                                workOrders={form.workOrders}
                                type={WorkOrderTypeName.NaturalLanguageProcess}
                              />
                            </Tab>
                          </Show> */}
                        </>
                      }
                    >
                      <Show visible={active === 'summary'}>
                        <ContentStoryForm
                          contentType={props.values.contentType}
                          checkTags={(c: string[]) => parseTags(c)}
                        />
                      </Show>
                      <Show visible={active === 'transcript'}>
                        <ContentTranscriptForm />
                      </Show>
                      <Show visible={active === 'clips'}>
                        <ContentClipForm
                          content={form}
                          setContent={setForm}
                          setClipErrors={setClipErrors}
                        />
                      </Show>
                      <Show visible={active === 'labels'}>
                        <ContentLabelsForm />
                      </Show>
                    </Tabs>
                  </Show>
                  <Show visible={props.values.contentType === ContentTypeName.PrintContent}>
                    <ContentStoryForm
                      contentType={props.values.contentType}
                      checkTags={(c: string[]) => parseTags(c)}
                    />
                  </Show>
                  <Show visible={props.values.contentType === ContentTypeName.AudioVideo}>
                    <Upload
                      className="media"
                      contentType={props.values.contentType}
                      id="upload"
                      name="file"
                      file={file}
                      stream={stream}
                      downloadable={fileReference?.isUploaded}
                      onSelect={(e) => {
                        const file = (e as IFile).name ? (e as IFile) : undefined;
                        props.setFieldValue('file', file);
                        // Remove file reference.
                        props.setFieldValue('fileReferences', []);
                      }}
                      onDownload={() => {
                        download(
                          props.values.id,
                          file?.name ?? `${props.values.otherSource}-${props.values.id}`,
                        );
                      }}
                      onDelete={() => {
                        setStream(undefined);
                      }}
                    />
                  </Show>
                </Row>
                <Row gap="0.5rem">
                  <Tags selectedExternal={parsedTags} />
                  <Show visible={props.values.contentType !== ContentTypeName.Image}>
                    <FormikSentiment name="tonePools" options={tonePools} required />
                    <Show
                      visible={
                        props.values.contentType === ContentTypeName.AudioVideo ||
                        props.values.contentType === ContentTypeName.PrintContent
                      }
                    >
                      <TimeLogSection
                        prepTimeRequired={props.values.contentType === ContentTypeName.AudioVideo}
                      />
                    </Show>
                  </Show>

                  <Row className="submit-buttons" gap="0.5rem">
                    <Col>
                      <Show
                        visible={
                          props.values.contentType === ContentTypeName.AudioVideo &&
                          props.values.fileReferences.length === 0 &&
                          !props.values.file
                        }
                      >
                        <Checkbox
                          name="allowPublishWithoutFile"
                          label="Allow publish without file"
                          className="allow-no-file"
                          checked={allowPublishWithoutFile}
                          onChange={(e) => {
                            setAllowPublishWithoutFile(e.target.checked);
                          }}
                        />
                      </Show>
                    </Col>
                    <Row gap="0.5rem">
                      <ContentNavigation
                        values={props.values}
                        fetchContent={fetchContent}
                        combinedPath={combinedPath}
                      />
                      <Button
                        type="submit"
                        disabled={
                          props.isSubmitting ||
                          ([ContentTypeName.AudioVideo, ContentTypeName.Image].includes(
                            props.values.contentType,
                          ) &&
                            !allowPublishWithoutFile &&
                            props.values.fileReferences.length === 0 &&
                            !props.values.file)
                        }
                        onClick={() => {
                          setSavePressed(true);
                        }}
                      >
                        Publish
                      </Button>
                      <Show
                        visible={
                          props.values.status === ContentStatusName.Publish ||
                          props.values.status === ContentStatusName.Published
                        }
                      >
                        <Button
                          variant={ButtonVariant.secondary}
                          disabled={
                            props.isSubmitting ||
                            (props.values.contentType === ContentTypeName.AudioVideo &&
                              props.values.fileReferences.length === 0 &&
                              !props.values.file)
                          }
                          onClick={() => {
                            setSavePressed(true);
                            handleUnpublish(props);
                          }}
                        >
                          Unpublish
                        </Button>
                      </Show>
                      <Show
                        visible={
                          ![ContentStatusName.Publish, ContentStatusName.Published].some(
                            (s) => s === props.values.status,
                          )
                        }
                      >
                        <Button
                          variant={ButtonVariant.warning}
                          disabled={props.isSubmitting}
                          onClick={() => {
                            setSavePressed(true);
                            handleSave(props);
                          }}
                        >
                          Save without publishing
                        </Button>
                      </Show>
                      <Show visible={!!props.values.id}>
                        <Show
                          visible={
                            !!props.values.id &&
                            props.values.contentType === ContentTypeName.AudioVideo
                          }
                        >
                          <Button
                            onClick={() =>
                              isWorkOrderStatus(form.workOrders, WorkOrderTypeName.Transcription, [
                                WorkOrderStatusName.Completed,
                              ])
                                ? toggleTranscribe()
                                : handleTranscribe(props.values, props)
                            }
                            variant={ButtonVariant.action}
                            disabled={
                              props.isSubmitting ||
                              !props.values.fileReferences.length ||
                              (props.values.fileReferences.length > 0 &&
                                !props.values.fileReferences[0].isUploaded)
                            }
                          >
                            Transcribe
                          </Button>
                        </Show>
                        <Show
                          visible={
                            props.values.status === ContentStatusName.Draft ||
                            (props.values.status === ContentStatusName.Unpublished &&
                              userInfo?.roles.includes(Claim.administrator))
                          }
                        >
                          <Button
                            onClick={toggleDelete}
                            variant={ButtonVariant.danger}
                            disabled={props.isSubmitting}
                          >
                            Delete
                          </Button>
                        </Show>
                        <Button variant={ButtonVariant.cyan} onClick={() => goToNext(form)}>
                          Next Snippet
                        </Button>
                      </Show>
                    </Row>
                  </Row>
                </Row>
                <Modal
                  headerText="Confirm Removal"
                  body="Are you sure you wish to delete this content?"
                  isShowing={showDeleteModal}
                  hide={toggleDelete}
                  type="delete"
                  confirmText="Yes, Delete It"
                  onConfirm={async () => {
                    try {
                      await deleteContent(toModel(props.values));
                    } finally {
                      toggleDelete();
                      navigate('/contents');
                    }
                  }}
                />
                <Modal
                  headerText="Confirm Transcript Request"
                  body="Content has already been transcribed, do you want to transcribe again?"
                  isShowing={showTranscribeModal}
                  hide={toggleTranscribe}
                  type="default"
                  confirmText="Yes, transcribe"
                  onConfirm={async () => {
                    try {
                      await handleTranscribe(props.values, props);
                    } finally {
                      toggleTranscribe();
                    }
                  }}
                />
                <Modal
                  headerText="Confirm NLP Request"
                  body="Content has already been Natural Language Processed, do you want to process again?"
                  isShowing={showNLPModal}
                  hide={toggleNLP}
                  type="default"
                  confirmText="Yes, process"
                  onConfirm={async () => {
                    try {
                      await handleNLP(props.values, props);
                    } finally {
                      toggleNLP();
                    }
                  }}
                />
              </Col>
            )}
          </FormikForm>
        </Area>
      </FormPage>
    </styled.ContentForm>
  );
};

export default ContentForm;
