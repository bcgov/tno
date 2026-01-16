import { FormikForm } from 'components/formik';
import { FormPage } from 'components/formpage';
import { FormikProps } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaBars, FaCopy, FaExternalLinkAlt } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import CreatableSelect from 'react-select/creatable';
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
  hasErrors,
  IOptionItem,
  ISeriesModel,
  ITimeTrackingModel,
  Modal,
  OptionItem,
  Row,
  Show,
  Tab,
  Tabs,
  TimeInput,
  toTitleCase,
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
  ContentQuotesForm,
  ContentStoryForm,
  ContentTranscriptForm,
} from '.';
import { ContentFormToolBar, IFile, Tags, TimeLogSection, Topic, Upload } from './components';
import { useContentForm } from './hooks';
import { ImageSection } from './ImageSection';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { getTargetField, setTime, toModel } from './utils';
import { WorkOrderStatus } from './WorkOrderStatus';

export interface IContentFormProps {
  /** Control what form elements are visible. */
  contentType?: ContentTypeName;
  /** Whether to scroll to content when loaded. */
  scrollToContent?: boolean;
  /** Root path for combined view. */
  combinedPath?: string;
}

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
    handleAutoClip,
    handleNLP,
    goToNext,
    file,
    fileReference,
    stream,
    setStream,
    setAvStream,
    download,
  } = useContentForm({
    contentType: initContentType,
    combinedPath,
  });
  const [{ contributorOptions, sources, series, sourceOptions, mediaTypeOptions }] =
    useLookupOptions();
  const [{ tonePools }] = useLookup();
  const { setShowValidationToast } = useTabValidationToasts();
  const { id } = useParams();
  const { isShowing: showDeleteModal, toggle: toggleDelete } = useModal();
  const { isShowing: showTranscribeModal, toggle: toggleTranscribe } = useModal();
  const { isShowing: showAutoClipModal, toggle: toggleAutoClip } = useModal();
  const { isShowing: showNLPModal, toggle: toggleNLP } = useModal();

  const refForm = React.useRef<HTMLDivElement>(null);

  const [size, setSize] = React.useState(1);
  const [active, setActive] = React.useState('summary');
  const [allowPublishWithoutFile, setAllowPublishWithoutFile] = React.useState(false);
  const [, setClipErrors] = React.useState<string>('');
  const [textDecorationStyle, setTextDecorationStyle] = React.useState('none');
  const [cursorStyle, setCursorStyle] = React.useState('text');
  const [savePressed, setSavePressed] = React.useState(false);
  const [parsedTags, setParsedTags] = React.useState<string[]>([]);
  const [useTitleCase, setUseTitleCase] = React.useState(true);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOtherOptions, setSeriesOtherOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOtherCreated, setSeriesOtherCreated] = React.useState<string>('');
  const [contentPrepTime, setContentPrepTime] = React.useState<string>('');

  const contentId = parseInt(id ?? '0');
  const urlParams = new URLSearchParams(window.location.search);
  const showPostedOn = ['', 'true'].includes(urlParams.get('showPostedOn') ?? 'false');

  // Function to filter series based on source and other criteria
  const filterSeries = React.useCallback(
    (sourceId: number | '', seriesId: number | '', isOther: boolean = false) => {
      return series.filter(
        (f) =>
          f.isOther === isOther &&
          ((!!seriesId && f.id === seriesId) || // Include current selected
            (f.isEnabled && (!sourceId || !f.sourceId || f.sourceId === sourceId))),
      );
    },
    [series],
  );

  // Function to create series options based on source and other criteria
  const filterSeriesOptions = React.useCallback(
    (sourceId: number | '', seriesId: number | '') => {
      const options = filterSeries(sourceId, seriesId, false).map(
        (m) => new OptionItem<number | ''>(m.name, m.id, !m.isEnabled),
      );
      setSeriesOptions(options);
    },
    [filterSeries],
  );

  // Function to create other series options based on source and other criteria
  const filterSeriesOtherOptions = React.useCallback(
    (sourceId: number | '', seriesId: number | '') => {
      let options = filterSeries(sourceId, seriesId, true).map(
        (m) => new OptionItem<number | ''>(m.name, m.id, !m.isEnabled),
      );
      if (seriesOtherCreated) options = options.concat(new OptionItem(seriesOtherCreated, ''));
      setSeriesOtherOptions(options);
    },
    [filterSeries, seriesOtherCreated],
  );

  React.useEffect(() => {
    filterSeriesOptions(form.sourceId, form.seriesId);
  }, [filterSeriesOptions, form.seriesId, form.sourceId]);

  React.useEffect(() => {
    filterSeriesOtherOptions(form.sourceId, form.seriesId);
  }, [filterSeriesOtherOptions, form.sourceId, form.seriesId]);

  React.useEffect(() => {
    if (contentId > 0 && contentId !== form.id) {
      fetchContent(contentId);
    }
  }, [form.id, fetchContent, setForm, contentId]);

  React.useEffect(() => {
    setAvStream();
  }, [setAvStream]);

  const onPrepTimeChanged = React.useCallback((value: string) => {
    setContentPrepTime(value);
  }, []);

  return (
    <styled.ContentForm className="content-form fvh" ref={refForm}>
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
            {(props: FormikProps<IContentForm>) => {
              const source = sources.find((s) => s.id === props.values.sourceId);
              const program = series.find((s) => s.id === props.values.seriesId);

              return (
                <Col className="content-col fvh">
                  <ContentFormToolBar
                    fetchContent={fetchContent}
                    combinedPath={combinedPath}
                    ref={refForm}
                  />
                  <FormikHidden name="uid" formNoValidate />
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
                        <Row>
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
                                    sourceOptions.find(
                                      (mt) => mt.value === props.values.sourceId,
                                    ) ?? ''
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
                                    filterSeriesOptions(
                                      newValue?.value ?? '',
                                      props.values.seriesId,
                                    );
                                    filterSeriesOtherOptions(
                                      newValue?.value ?? '',
                                      props.values.seriesId,
                                    );
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
                                  // Set a unique key for the component
                                  // This ensures that the component is re-rendered when the mediaTypeId changes
                                  key={`mediaType-${props.values.mediaTypeId}`}
                                  // Set the default value for the media type select
                                  defaultValue={mediaTypeOptions.find(
                                    (mt) => mt.value === props.values.mediaTypeId,
                                  )}
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
                            <Row alignItems="center">
                              <FormikText
                                name="byline"
                                label="Byline"
                                width="50ch"
                                onChange={(e) => {
                                  const inputValue = e.target.value;

                                  const matchedOption = contributorOptions.find(
                                    (option) =>
                                      typeof option.label === 'string' &&
                                      option.label.toLowerCase() === inputValue.toLowerCase(),
                                  );

                                  if (matchedOption) {
                                    props.setFieldValue('contributorId', matchedOption.value);
                                  } else {
                                    props.setFieldValue('contributorId', '');
                                  }

                                  if (useTitleCase) {
                                    props.setFieldValue('byline', toTitleCase(e.target.value));
                                  } else {
                                    props.setFieldValue('byline', e.target.value);
                                  }
                                }}
                              />
                              <Checkbox
                                label="Title Case"
                                name="useTitleCase"
                                checked={useTitleCase}
                                onChange={(e) => {
                                  setUseTitleCase(e.target.checked);
                                }}
                                className="checkbox-byline"
                              />
                            </Row>
                            <FormikSelect
                              name="contributorId"
                              value={
                                contributorOptions.find(
                                  (mt) => mt.value === props.values.contributorId,
                                ) ?? ''
                              }
                              label="Columnist/Pundit"
                              width={FieldSize.Medium}
                              options={contributorOptions}
                            />
                            <FormikText name="edition" label="Edition" />
                            <FormikText name="section" label="Section" />
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
                            <Row alignItems="center">
                              <FormikText
                                name="byline"
                                label="Byline"
                                onChange={(e) => {
                                  const inputValue = e.target.value;

                                  const matchedOption = contributorOptions.find(
                                    (option) =>
                                      typeof option.label === 'string' &&
                                      option.label.toLowerCase() === inputValue.toLowerCase(),
                                  );

                                  if (matchedOption) {
                                    props.setFieldValue('contributorId', matchedOption.value);
                                  } else {
                                    props.setFieldValue('contributorId', '');
                                  }

                                  if (useTitleCase) {
                                    props.setFieldValue('byline', toTitleCase(e.target.value));
                                  } else {
                                    props.setFieldValue('byline', e.target.value);
                                  }
                                }}
                              />
                              <Checkbox
                                label="Title Case"
                                name="useTitleCase"
                                checked={useTitleCase}
                                onChange={(e) => {
                                  setUseTitleCase(e.target.checked);
                                }}
                                className="checkbox-byline"
                              />
                            </Row>
                            <FormikSelect
                              name="contributorId"
                              value={
                                contributorOptions.find(
                                  (mt) => mt.value === props.values.contributorId,
                                ) ?? ''
                              }
                              label="Columnist/Pundit"
                              width={FieldSize.Medium}
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
                                  if (textDecorationStyle !== 'none')
                                    setTextDecorationStyle('none');
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
                                  width="8em"
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
                                    const date = setTime(props.values.publishedOn, e.target.value);
                                    if (!!date) {
                                      props.setFieldValue(
                                        'publishedOn',
                                        moment(date).format('MMM D, yyyy HH:mm:ss'),
                                      );
                                      props.setFieldValue(
                                        'publishedOnTime',
                                        moment(date).format('HH:mm:ss'),
                                      );
                                    }
                                  }}
                                  onChange={(e) => {
                                    const date = setTime(props.values.publishedOn, e.target.value);
                                    if (!!date) {
                                      props.setFieldValue(
                                        'publishedOn',
                                        moment(date).format('MMM D, yyyy HH:mm:ss'),
                                      );
                                      props.setFieldValue(
                                        'publishedOnTime',
                                        moment(date).format('HH:mm:ss'),
                                      );
                                    }
                                  }}
                                />
                              </Show>
                              <Show visible={showPostedOn}>
                                <FormikDatePicker
                                  name="postedOn"
                                  label="Posted On"
                                  autoComplete="false"
                                  width={FieldSize.Medium}
                                  selectedDate={
                                    !!props.values.postedOn
                                      ? moment(props.values.postedOn).toString()
                                      : undefined
                                  }
                                  value={
                                    !!props.values.postedOn
                                      ? moment(props.values.postedOn).format('MMM D, yyyy')
                                      : ''
                                  }
                                  onChange={(date) => {
                                    if (!!props.values.postedOnTime) {
                                      const hours = props.values.postedOnTime?.split(':');
                                      if (!!hours && !!date) {
                                        date.setHours(
                                          Number(hours[0]),
                                          Number(hours[1]),
                                          Number(hours[2]),
                                        );
                                      }
                                    }
                                    props.setFieldValue(
                                      'postedOn',
                                      moment(date).format('MMM D, yyyy HH:mm:ss'),
                                    );
                                  }}
                                />
                                <TimeInput
                                  name="postedOnTime"
                                  label="Time"
                                  disabled={!props.values.postedOn}
                                  width="7em"
                                  value={!!props.values.postedOn ? props.values.postedOnTime : ''}
                                  placeholder={
                                    !!props.values.postedOn ? props.values.postedOnTime : 'HH:MM:SS'
                                  }
                                  onBlur={(e) => {
                                    if (e.target.value.indexOf('_')) {
                                      e.target.value = e.target.value.replaceAll('_', '0');
                                    }
                                    const date = setTime(
                                      props.values.postedOn ?? '',
                                      e.target.value,
                                    );
                                    if (!!date) {
                                      props.setFieldValue(
                                        'postedOn',
                                        moment(date).format('MMM D, yyyy HH:mm:ss'),
                                      );
                                      props.setFieldValue(
                                        'postedOnTime',
                                        moment(date).format('HH:mm:ss'),
                                      );
                                    }
                                  }}
                                  onChange={(e) => {
                                    const date = setTime(
                                      props.values.postedOn ?? '',
                                      e.target.value,
                                    );
                                    if (!!date) {
                                      props.setFieldValue(
                                        'postedOn',
                                        moment(date).format('MMM D, yyyy HH:mm:ss'),
                                      );
                                      props.setFieldValue(
                                        'postedOnTime',
                                        moment(date).format('HH:mm:ss'),
                                      );
                                    }
                                  }}
                                />
                              </Show>
                              <Show
                                visible={props.values.contentType === ContentTypeName.AudioVideo}
                              >
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
                                    options={seriesOptions}
                                    isDisabled={!!props.values.otherSeries}
                                    onChange={(e) => {
                                      props.setFieldValue('otherSeries', '');
                                    }}
                                  />
                                  <div className="frm-in">
                                    <label htmlFor="selOtherSeries">Other Show/Program</label>
                                    <CreatableSelect
                                      name={'selOtherSeries'}
                                      className={'other-series-select'}
                                      isClearable
                                      options={seriesOtherOptions}
                                      onChange={(e: any) => {
                                        let foundSeries: ISeriesModel | undefined;
                                        foundSeries = series.find((c) => c.id === e?.value);
                                        if (e?.value == null || e?.value === '') {
                                          // Clear selected value
                                          props.setFieldValue('otherSeries', '');
                                          props.setFieldValue('seriesId', '');
                                          setSeriesOtherCreated('');
                                        } else if (!!foundSeries && foundSeries.isOther) {
                                          // this is a known "Other Series"
                                          props.setFieldValue('otherSeries', '');
                                          props.setFieldValue('seriesId', foundSeries.id);
                                          setSeriesOtherCreated('');
                                        }
                                      }}
                                      onCreateOption={(inputValue: string) => {
                                        // this is a "created" option, but we need to check if
                                        // it's a duplicate of an existing option this is !isOther
                                        const value = inputValue.trim();
                                        let foundSeries: ISeriesModel | undefined = series.find(
                                          (s) =>
                                            s.name.toLocaleLowerCase() ===
                                            value.toLocaleLowerCase(),
                                        );
                                        if (!!foundSeries) {
                                          // this is an existing series - not isOther
                                          props.setFieldValue('seriesId', foundSeries.id);
                                          props.setFieldValue('otherSeries', '');
                                          setSeriesOtherCreated('');
                                        } else {
                                          // this is a new "other" series
                                          props.setFieldValue('otherSeries', value);
                                          props.setFieldValue('seriesId', undefined);
                                          // save the new created series/program name
                                          setSeriesOtherCreated(value);
                                        }
                                      }}
                                      value={
                                        seriesOtherOptions.find(
                                          (s: any) =>
                                            (props.values.otherSeries.length > 0 &&
                                              s.label === props.values.otherSeries) ||
                                            s.value === props.values.seriesId,
                                        ) ?? ''
                                      }
                                    />
                                  </div>
                                  {/* Temporarily hidden to avoid confusing Editors */}
                                  {/* <FormikCheckbox
                                    label="Is CBRA Unqualified"
                                    name="isCBRAUnqualified"
                                    className="checkbox-cbra"
                                  /> */}
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
                              <div className="frm-in">
                                <label htmlFor="selTopic">Topic</label>
                                <Topic
                                  name={'selTopic'}
                                  className={'topic-select'}
                                  value={
                                    !!props.values.topics?.length
                                      ? props.values.topics[0].id
                                      : undefined
                                  }
                                  isDisabled={!props.values.sourceId}
                                  handleTopicChange={(topic) => {
                                    props.setFieldValue('topics', [topic]);
                                  }}
                                />
                              </div>
                            </Row>
                          </Show>
                        </Row>
                      </Col>
                    </Show>
                  </Row>
                  <Show visible={props.values.contentType === ContentTypeName.Image}>
                    <Row flex="1" wrap="nowrap" gap="0.5rem" className="section-upload">
                      <ContentStoryForm
                        contentType={ContentTypeName.Image}
                        setParsedTags={setParsedTags}
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
                    </Row>
                  </Show>
                  <Row
                    flex="1"
                    wrap="nowrap"
                    gap="0.5rem"
                    className="section-upload tab-section fvh"
                  >
                    <Show visible={props.values.contentType !== ContentTypeName.Image}>
                      <Col flex="1 1 auto" justifyContent="center">
                        <Tabs
                          className={`fvh ${size === 1 ? 'small' : 'large'} tab-layout`}
                          tabs={
                            <>
                              <Tab
                                label={
                                  props.values.contentType === ContentTypeName.PrintContent ||
                                  props.values.contentType === ContentTypeName.Internet
                                    ? 'Story'
                                    : 'Summary'
                                }
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
                              <Tab
                                label={`Quotes (${props.values.quotes.length})`}
                                onClick={() => setActive('quotes')}
                                active={active === 'quotes'}
                              />
                              <Show
                                visible={props.values.contentType === ContentTypeName.AudioVideo}
                              >
                                <Tab
                                  onClick={() => setActive('transcript')}
                                  active={active === 'transcript'}
                                >
                                  <Row alignItems="center" gap="0.25rem">
                                    <WorkOrderStatus
                                      workOrders={form.workOrders}
                                      type={[
                                        WorkOrderTypeName.Transcription,
                                        WorkOrderTypeName.AutoClip,
                                      ]}
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
                              setParsedTags={setParsedTags}
                            />
                          </Show>
                          <Show visible={active === 'quotes'}>
                            <ContentQuotesForm />
                          </Show>
                          <Show visible={active === 'transcript'}>
                            <ContentTranscriptForm setParsedTags={setParsedTags} />
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
                      </Col>
                    </Show>
                    {/** Audio Video layout */}
                    <Show visible={props.values.contentType === ContentTypeName.AudioVideo}>
                      <Col flex="0 0 35rem" justifyContent="center">
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
                      </Col>
                    </Show>
                  </Row>
                  <Row gap="0.5rem">
                    <Tags
                      defaultTags={parsedTags}
                      targetField={getTargetField(props.values.contentType, active)}
                      enableAutoTagText={true}
                    />
                    <FormikSentiment name="tonePools" options={tonePools} required />
                    <Show
                      visible={
                        props.values.contentType === ContentTypeName.AudioVideo ||
                        props.values.contentType === ContentTypeName.PrintContent
                      }
                    >
                      <TimeLogSection
                        prepTimeRequired={props.values.contentType === ContentTypeName.AudioVideo}
                        prepTime={contentPrepTime}
                        onPrepTimeChanged={onPrepTimeChanged}
                      />
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
                            if (contentPrepTime && Number.parseInt(contentPrepTime) > 0) {
                              const userId = userInfo?.id ?? 0;
                              const entry: ITimeTrackingModel = {
                                id: 0,
                                contentId: props.values.id,
                                userId: userId,
                                activity: !!props.values.id ? 'Updated' : 'Created',
                                effort: +Number.parseInt(contentPrepTime),
                                createdOn: moment().toLocaleString(),
                              };
                              // remove added but not saved entry
                              const addedEntryIndex = props.values.timeTrackings.findIndex(
                                (entry) => entry.id === 0,
                              );
                              if (addedEntryIndex !== -1) {
                                props.setFieldValue('timeTrackings', [
                                  ...props.values.timeTrackings.slice(0, addedEntryIndex),
                                  ...props.values.timeTrackings.slice(addedEntryIndex + 1),
                                ]);
                              }
                              // insert the new entry
                              props.setFieldValue('timeTrackings', [
                                ...props.values.timeTrackings,
                                entry,
                              ]);
                              setContentPrepTime('');
                            }
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
                                isWorkOrderStatus(
                                  form.workOrders,
                                  [WorkOrderTypeName.Transcription, WorkOrderTypeName.AutoClip],
                                  [WorkOrderStatusName.Completed],
                                )
                                  ? toggleTranscribe()
                                  : handleTranscribe(props.values, props)
                              }
                              variant={ButtonVariant.action}
                              disabled={
                                props.values.isApproved ||
                                props.isSubmitting ||
                                !props.values.fileReferences.length ||
                                (props.values.fileReferences.length > 0 &&
                                  !props.values.fileReferences[0].isUploaded)
                              }
                            >
                              Transcribe
                            </Button>
                            <Button
                              onClick={() =>
                                isWorkOrderStatus(
                                  form.workOrders,
                                  [WorkOrderTypeName.Transcription, WorkOrderTypeName.AutoClip],
                                  [WorkOrderStatusName.Completed],
                                )
                                  ? toggleAutoClip()
                                  : handleAutoClip(props.values, props)
                              }
                              variant={ButtonVariant.action}
                              disabled={
                                props.values.isApproved ||
                                props.isSubmitting ||
                                !props.values.fileReferences.length ||
                                (props.values.fileReferences.length > 0 &&
                                  !props.values.fileReferences[0].isUploaded)
                              }
                            >
                              Clip
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
                          <Show
                            visible={
                              !!props.values.id &&
                              props.values.contentType === ContentTypeName.AudioVideo
                            }
                          >
                            <Button
                              variant={ButtonVariant.cyan}
                              onClick={() => goToNext(props.values)}
                            >
                              Next Snippet
                            </Button>
                          </Show>
                        </Show>
                        <ContentNavigation
                          values={props.values}
                          fetchContent={fetchContent}
                          showRefresh={false}
                        />
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
                    headerText="Confirm Auto Clip Request"
                    body="Content has already been auto clipped, do you want to auto clip again?"
                    isShowing={showAutoClipModal}
                    hide={toggleAutoClip}
                    type="default"
                    confirmText="Yes, auto clip"
                    onConfirm={async () => {
                      try {
                        await handleAutoClip(props.values, props);
                      } finally {
                        toggleAutoClip();
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
              );
            }}
          </FormikForm>
        </Area>
      </FormPage>
    </styled.ContentForm>
  );
};

export default ContentForm;
