import { AxiosError } from 'axios';
import { FormikForm } from 'components/formik';
import { FormikHelpers, FormikProps } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaBars, FaCopy, FaExternalLinkAlt } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import {
  HubMethodName,
  useApiHub,
  useApp,
  useContent,
  useLookupOptions,
  useWorkOrders,
} from 'store/hooks';
import { IAjaxRequest } from 'store/slices';
import {
  Area,
  Button,
  ButtonVariant,
  Claim,
  Col,
  ContentStatusName,
  ContentTypeName,
  FieldSize,
  filterEnabledOptions,
  FormikCheckbox,
  FormikHidden,
  FormikSelect,
  FormikText,
  FormikTextArea,
  FormPage,
  hasErrors,
  IContentMessageModel,
  IContentModel,
  IResponseErrorModel,
  IWorkOrderMessageModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useCombinedView,
  useModal,
  useTabValidationToasts,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { isWorkOrderStatus } from '../utils';
import { ContentFormSchema } from '../validation';
import { ContentClipForm, ContentLabelsForm, ContentStoryForm, ContentTranscriptForm } from '.';
import { ContentFormToolBar, Tags, TimeLogSection, ToningGroup } from './components';
import { defaultFormValues } from './constants';
import { ImageSection } from './ImageSection';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { getContentPath, toForm, toModel, triggerFormikValidate } from './utils';
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
  const hub = useApiHub();
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { getContent, addContent, updateContent, deleteContent, upload, attach }] = useContent();
  const [, { findWorkOrders, transcribe, nlp }] = useWorkOrders();
  const { isShowing: showDeleteModal, toggle: toggleDelete } = useModal();
  const { isShowing: showTranscribeModal, toggle: toggleTranscribe } = useModal();
  const { isShowing: showNLPModal, toggle: toggleNLP } = useModal();
  const [{ contributorOptions, sources, series, sourceOptions, productOptions }, { getSeries }] =
    useLookupOptions();
  const { combined, formType } = useCombinedView(initContentType);
  const { setShowValidationToast } = useTabValidationToasts();

  const [contentType, setContentType] = React.useState(formType ?? initContentType);
  const [size, setSize] = React.useState(1); // TODO: change this to use css media types instead.
  const [active, setActive] = React.useState('properties');
  const [savePressed, setSavePressed] = React.useState(false);
  const [allowPublishWithoutFile, setAllowPublishWithoutFile] = React.useState(false);
  const [, setClipErrors] = React.useState<string>('');
  const [textDecorationStyle, setTextDecorationStyle] = React.useState('none');
  const [cursorStyle, setCursorStyle] = React.useState('text');
  const [form, setForm] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
    id: parseInt(id ?? '0'),
  });
  const [isSubmitting, setIsSubmitting] = React.useState(false);

  const userId = userInfo?.id ?? '';

  const updateForm = React.useCallback(
    async (content: IContentModel | undefined) => {
      if (!!content) {
        setForm(toForm(content));
        const res = await findWorkOrders({ contentId: content.id });
        setForm({ ...toForm(content), workOrders: res.data.items });
        // If the form is loaded from the URL instead of clicking on the list view it defaults to the snippet form.
        setContentType(content.contentType);
      }
    },
    [findWorkOrders],
  );

  const fetchContent = React.useCallback(
    async (id: number) => {
      try {
        const content = await getContent(id);
        await updateForm(content);
      } catch (error) {
        const aError = error as AxiosError;
        if (!!aError && !!aError.response?.data) {
          const data = aError.response.data as IResponseErrorModel;
          if (data.type === 'NoContentException') navigate('/contents');
        }
      }
    },
    [getContent, updateForm, navigate],
  );

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderMessageModel) => {
      if (form.id === workOrder.configuration.contentId)
        // TODO: Don't overwrite the user's edits.
        fetchContent(workOrder.configuration.contentId);
    },
    [fetchContent, form],
  );

  hub.useHubEffect(HubMethodName.WorkOrder, onWorkOrder);

  const onContentUpdated = React.useCallback(
    async (message: IContentMessageModel) => {
      if (form.id === message.id && form.version !== message.version && !isSubmitting) {
        try {
          // TODO: Don't overwrite the user's edits.
          fetchContent(message.id);
        } catch {}
      }
    },
    [fetchContent, form.id, form.version, isSubmitting],
  );

  hub.useHubEffect(HubMethodName.ContentUpdated, onContentUpdated);

  React.useEffect(() => {
    if (!!id && +id > 0) {
      fetchContent(+id);
    }
  }, [id, fetchContent]);

  const handleSubmit = React.useCallback(
    async (
      values: IContentForm,
      formikHelpers: FormikHelpers<IContentForm>,
    ): Promise<IContentForm> => {
      setIsSubmitting(true);
      let contentResult: IContentModel | null = null;
      const originalId = values.id;
      let result = form;
      try {
        if (!values.id) {
          // Only new content is initialized.
          values.contentType = contentType;
          values.ownerId = userId;
        }

        const model = toModel(values);
        contentResult = !form.id ? await addContent(model) : await updateContent(model);

        if (!!values.file) {
          // TODO: Make it possible to upload on the initial save instead of a separate request.
          // Upload the file if one has been added.
          const content = await upload(contentResult, values.file);
          result = toForm({ ...content, tonePools: values.tonePools });
        } else if (
          !originalId &&
          !!values.fileReferences.length &&
          !values.fileReferences[0].isUploaded
        ) {
          // TODO: Make it possible to upload on the initial save instead of a separate request.
          // A file was attached but hasn't been uploaded to the API.
          const fileReference = values.fileReferences[0];
          const content = await attach(contentResult.id, 0, fileReference.path);
          result = toForm({ ...content, tonePools: values.tonePools });
        } else {
          result = toForm({ ...contentResult, tonePools: values.tonePools });
        }
        setForm({ ...result, workOrders: form.workOrders });

        toast.success(`"${contentResult.headline}" has successfully been saved.`);

        if (!originalId) {
          if (result.status === ContentStatusName.Draft)
            navigate(getContentPath(combined, contentResult.id, contentResult?.contentType));
          else {
            // Reset form for next record.
            const parsedDate = moment(values.publishedOn, 'MMM D, yyyy HH:mm:ss');
            const updatedDate = parsedDate.add(1, 'second');
            setForm({
              ...defaultFormValues(contentType),
              sourceId: values.sourceId,
              productId: values.productId,
              otherSource: values.otherSource,
              publishedOn: updatedDate.format('MMM D, yyyy HH:mm:ss'),
            });
          }
        }
        if (!!contentResult?.seriesId) {
          // A dynamically added series has been added, fetch the latests series.
          const newSeries = series.find((s) => s.id === contentResult?.seriesId);
          if (!newSeries) getSeries();
        }
      } catch {
        // If the upload fails, we still need to update the form from the original update.
        if (!!contentResult) {
          result = toForm(contentResult);
          setForm({ ...result, workOrders: form.workOrders });
          if (!originalId)
            navigate(getContentPath(combined, contentResult.id, contentResult?.contentType));
        }
      } finally {
        setIsSubmitting(false);
      }
      return result;
    },
    [
      addContent,
      attach,
      combined,
      contentType,
      form,
      getSeries,
      navigate,
      series,
      updateContent,
      upload,
      userId,
    ],
  );

  const handlePublish = React.useCallback(
    async (
      values: IContentForm,
      formikHelpers: FormikHelpers<IContentForm>,
    ): Promise<IContentForm> => {
      if (
        [
          ContentStatusName.Draft,
          ContentStatusName.Unpublish,
          ContentStatusName.Unpublished,
        ].includes(values.status)
      )
        values.status = ContentStatusName.Publish;

      return await handleSubmit(values, formikHelpers);
    },
    [handleSubmit],
  );

  const handleUnpublish = React.useCallback(
    async (props: FormikProps<IContentForm>) => {
      if (
        props.values.status === ContentStatusName.Publish ||
        props.values.status === ContentStatusName.Published
      ) {
        triggerFormikValidate(props);
        if (props.isValid) {
          props.values.status = ContentStatusName.Unpublish;
          await handleSubmit(props.values, props);
        }
      }
    },
    [handleSubmit],
  );

  const handleSave = React.useCallback(
    async (props: FormikProps<IContentForm>) => {
      triggerFormikValidate(props);
      props.validateForm(props.values);
      if (props.isValid) {
        await handleSubmit(props.values, props);
      }
    },
    [handleSubmit],
  );

  const handleTranscribe = React.useCallback(
    async (values: IContentForm, formikHelpers: FormikHelpers<IContentForm>) => {
      try {
        // TODO: Only save when required.
        // Save before submitting request.
        const content = await handleSubmit(values, formikHelpers);
        const response = await transcribe(toModel(values));
        setForm({ ...content, workOrders: [response.data, ...form.workOrders] });

        if (response.status === 200) toast.success('A transcript has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been transcribed');
          else toast.warn(`An active request for transcription already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [form.workOrders, handleSubmit, transcribe],
  );

  const handleNLP = React.useCallback(
    async (values: IContentForm, formikHelpers: FormikHelpers<IContentForm>) => {
      try {
        // TODO: Only save when required.
        // Save before submitting request.
        const content = await handleSubmit(values, formikHelpers);
        const response = await nlp(toModel(values));
        setForm({ ...content, workOrders: [response.data, ...form.workOrders] });

        if (response.status === 200) toast.success('An NLP has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been processed by NLP');
          else toast.warn(`An active request for NLP already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [form.workOrders, handleSubmit, nlp],
  );

  return (
    <styled.ContentForm className="content-form">
      <FormPage className={combined ? 'no-padding' : ''}>
        <Area>
          <FormikForm
            onSubmit={handlePublish}
            validationSchema={ContentFormSchema}
            initialValues={form}
            loading={(request: IAjaxRequest) =>
              !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
            }
          >
            {(props: FormikProps<IContentForm>) => (
              <Col className="content-col">
                <ContentFormToolBar fetchContent={fetchContent} combinedPath={combinedPath} />
                <FormikHidden name="uid" />
                <Row alignItems="flex-start" className="content-details">
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
                          <Show visible={contentType !== ContentTypeName.Image}>
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
                                    if (!!source?.productId)
                                      props.setFieldValue('productId', source.productId);
                                  }
                                }}
                                options={filterEnabledOptions(
                                  sourceOptions,
                                  props.values.sourceId,
                                ).filter(
                                  (x) =>
                                    contentType !== ContentTypeName.Image ||
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
                                name="productId"
                                value={
                                  productOptions.find(
                                    (mt) => mt.value === props.values.productId,
                                  ) ?? ''
                                }
                                label="Product"
                                width={FieldSize.Small}
                                options={productOptions}
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
                      <Show visible={contentType === ContentTypeName.Image}>
                        <ImageSection />
                      </Show>
                      <Show visible={contentType === ContentTypeName.PrintContent}>
                        <Row>
                          <FormikText name="byline" label="Byline" required />
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
                          <FormikText name="section" label="Section" required />
                          <FormikText name="page" label="Page" />
                        </Row>
                      </Show>
                      <Show
                        visible={
                          contentType === ContentTypeName.AudioVideo ||
                          contentType === ContentTypeName.Story
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
                    </Col>
                  </Show>
                </Row>
                <Row flex="1 1 100%" wrap="nowrap">
                  <Show visible={contentType === ContentTypeName.Image}>
                    <ContentStoryForm contentType={ContentTypeName.Image} />
                  </Show>
                </Row>
                <Row className="tab-section">
                  <Show
                    visible={
                      contentType !== ContentTypeName.PrintContent &&
                      contentType !== ContentTypeName.Image
                    }
                  >
                    <Tabs
                      className={`'expand'} ${size === 1 ? 'small' : 'large'}`}
                      tabs={
                        <>
                          <Tab
                            label="Properties"
                            onClick={() => {
                              setActive('properties');
                            }}
                            active={active === 'properties'}
                            hasErrors={
                              hasErrors(props.errors, ['publishedOn', 'tone', 'summary']) &&
                              active !== 'properties'
                            }
                            showErrorOnSave={{ value: true, savePressed: savePressed }}
                            setShowValidationToast={setShowValidationToast}
                          />
                          <Show visible={props.values.contentType === ContentTypeName.AudioVideo}>
                            <Tab
                              onClick={() => setActive('transcript')}
                              active={active === 'transcript'}
                            >
                              <Row>
                                <span>Transcript</span>
                                <WorkOrderStatus
                                  workOrders={form.workOrders}
                                  type={WorkOrderTypeName.Transcription}
                                />
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
                      <Show visible={active === 'properties'}>
                        <ContentStoryForm contentType={contentType} />
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
                  <Show visible={contentType === ContentTypeName.PrintContent}>
                    <ContentStoryForm contentType={contentType} />
                  </Show>
                </Row>
                <Row gap="0.5rem">
                  <Tags />
                  <Show visible={contentType !== ContentTypeName.Image}>
                    <ToningGroup fieldName="tonePools" />
                    <Show
                      visible={
                        contentType === ContentTypeName.AudioVideo ||
                        contentType === ContentTypeName.PrintContent
                      }
                    >
                      <TimeLogSection
                        prepTimeRequired={contentType === ContentTypeName.AudioVideo}
                      />
                    </Show>
                  </Show>

                  <Row className="submit-buttons">
                    <Show
                      visible={
                        contentType === ContentTypeName.AudioVideo &&
                        props.values.fileReferences.length === 0 &&
                        !props.values.file
                      }
                    >
                      <FormikCheckbox
                        name="allowPublishWithoutFile"
                        label="Allow publish without file"
                        className="allow-no-file"
                        value={allowPublishWithoutFile}
                        checked={allowPublishWithoutFile}
                        onChange={(e: any) => {
                          setAllowPublishWithoutFile(e.target.checked);
                        }}
                      />
                    </Show>
                    <Row>
                      <Button
                        type="submit"
                        disabled={
                          props.isSubmitting ||
                          ([ContentTypeName.AudioVideo, ContentTypeName.Image].includes(
                            contentType,
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
                            (contentType === ContentTypeName.AudioVideo &&
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
