import { faTableColumns, faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { FormikProps } from 'formik';
import {
  ActionName,
  ContentTypeName,
  HubMethodName,
  IActionModel,
  IWorkOrderModel,
  useCombinedView,
  useLookupOptions,
  useTooltips,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'hooks';
import { ContentStatusName, IContentModel, useApiHub, ValueType } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import { useTabValidationToasts } from 'hooks/useTabValidationToasts';
import React from 'react';
import {
  FaBars,
  FaChevronLeft,
  FaChevronRight,
  FaCopy,
  FaExternalLinkAlt,
  FaSpinner,
} from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useWorkOrders } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import { IAjaxRequest } from 'store/slices';
import {
  Area,
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikHidden,
  FormikSelect,
  FormikText,
  FormikTextArea,
  FormPage,
  IconButton,
  Row,
  Show,
  Tab,
  Tabs,
} from 'tno-core';
import { hasErrors } from 'utils';

import { getStatusText } from '../list-view/utils';
import { ContentFormToolBar } from '../tool-bar/ContentFormToolBar';
import { isWorkOrderStatus } from '../utils';
import { ContentFormSchema } from '../validation';
import { ContentClipForm, ContentLabelsForm, ContentSummaryForm, ContentTranscriptForm } from '.';
import { defaultFormValues } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { getContentPath, toForm, toModel, triggerFormikValidate } from './utils';
import { WorkOrderStatus } from './WorkOrderStatus';

export interface IContentFormProps {
  /** Control what form elements are visible. */
  contentType?: ContentTypeName;
}

/**
 * Snippet Form edit and create form for default view. Path will be appended with content id.
 * @param param0 Component properties.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC<IContentFormProps> = ({
  contentType: initContentType = ContentTypeName.Snippet,
}) => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [
    { content: page },
    { getContent, addContent, updateContent, deleteContent, upload, attach },
  ] = useContent();
  const [, { findWorkOrders, transcribe, nlp }] = useWorkOrders();
  const { isShowing: showDeleteModal, toggle: toggleDelete } = useModal();
  const { isShowing: showTranscribeModal, toggle: toggleTranscribe } = useModal();
  const { isShowing: showNLPModal, toggle: toggleNLP } = useModal();
  const [{ sources, tonePools, series, sourceOptions, productOptions }, { getSeries }] =
    useLookupOptions();
  const { combined, formType } = useCombinedView(initContentType);
  const hub = useApiHub();
  useTooltips();

  const [contentType, setContentType] = React.useState(formType ?? initContentType);
  const [size, setSize] = React.useState(1);
  const [active, setActive] = React.useState('properties');
  const [savePressed, setSavePressed] = React.useState(false);
  const [clipErrors, setClipErrors] = React.useState<string>('');
  const [textDecorationStyle, setTextDecorationStyle] = React.useState('none');
  const [cursorStyle, setCursorStyle] = React.useState('text');
  const [form, setForm] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
    id: parseInt(id ?? '0'),
  });

  const userId = userInfo?.id ?? '';
  const indexPosition = !!id ? page?.items.findIndex((c) => c.id === +id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (page?.items.length ?? 0) - 1;

  const determineActions = () => {
    switch (contentType) {
      case ContentTypeName.PrintContent:
        return (a: IActionModel) =>
          a.valueType === ValueType.Boolean && a.name !== ActionName.NonQualified;
      case ContentTypeName.Image:
        return (a: IActionModel) => a.name === ActionName.FrontPage;
      case ContentTypeName.Snippet:
      // TODO: Determine actions for remaining content types
      // eslint-disable-next-line no-fallthrough
      default:
        return (a: IActionModel) =>
          a.valueType === ValueType.Boolean && a.name !== ActionName.Alert;
    }
  };

  const fetchContent = React.useCallback(
    (id: number) => {
      getContent(id).then((content) => {
        setForm(toForm(content));
        findWorkOrders({ contentId: id }).then((res) => {
          setForm({ ...toForm(content), workOrders: res.data.items });
          // If the form is loaded from the URL instead of clicking on the list view it defaults to the snippet form.
        });
        setContentType(content.contentType);
      });
    },
    [getContent, findWorkOrders],
  );

  const onWorkOrder = React.useCallback(
    (workOrder: IWorkOrderModel) => {
      if (!!id && +id === workOrder.configuration.contentId)
        fetchContent(workOrder.configuration.contentId);
    },
    [fetchContent, id],
  );

  React.useEffect(() => {
    return hub.listen(HubMethodName.WorkOrder, onWorkOrder);
  }, [hub, onWorkOrder]);

  React.useEffect(() => {
    if (!!id && +id > 0) {
      fetchContent(+id);
    }
  }, [id, fetchContent]);

  const { setShowValidationToast } = useTabValidationToasts();

  const handleSave = async (values: IContentForm): Promise<IContentForm> => {
    let contentResult: IContentModel | null = null;
    const originalId = values.id;
    let result = form;
    try {
      if (!values.id) {
        // Only new content is initialized.
        values.contentType = contentType;
        values.ownerId = userId;
      }

      const defaultTonePool = tonePools.find((t) => t.name === 'Default');
      values.tonePools = !!defaultTonePool ? [{ ...defaultTonePool, value: +values.tone }] : [];

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

      if (!originalId)
        navigate(
          `${getContentPath(contentResult?.contentType)}${combined ? 'combined/' : ''}${
            contentResult.id
          }`,
        );
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
        if (!originalId) navigate(`/contents/${combined ? 'combined/' : ''}${contentResult.id}`);
      }
    }
    return result;
  };

  const handlePublish = async (props: FormikProps<IContentForm>) => {
    // Change the status to publish if required.
    if (
      [
        ContentStatusName.Draft,
        ContentStatusName.Unpublish,
        ContentStatusName.Unpublished,
      ].includes(props.values.status)
    )
      props.values.status = ContentStatusName.Publish;

    triggerFormikValidate(props);
    if (props.isValid) {
      await handleSave(props.values);
    }
  };

  const handleUnpublish = async (props: FormikProps<IContentForm>) => {
    if (
      props.values.status === ContentStatusName.Publish ||
      props.values.status === ContentStatusName.Published
    )
      props.values.status = ContentStatusName.Unpublish;
    triggerFormikValidate(props);
    if (props.isValid) {
      await handleSave(props.values);
    }
  };

  const handleTranscribe = async (values: IContentForm) => {
    try {
      // TODO: Only save when required.
      // Save before submitting request.
      const content = await handleSave(values);
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
  };

  const handleNLP = async (values: IContentForm) => {
    try {
      // TODO: Only save when required.
      // Save before submitting request.
      const content = await handleSave(values);
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
  };

  return (
    <styled.ContentForm className="content-form">
      <FormPage minWidth={'1200px'} maxWidth="" className={combined ? 'no-padding' : ''}>
        <Area>
          <Row>
            <Show visible={!combined}>
              <IconButton label="List View" onClick={() => navigate('/contents')} iconType="back" />
            </Show>
            <Show visible={!combined}>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Combined View"
                onClick={() => {
                  navigate(`/contents/combined/${id}?form=${contentType}`);
                }}
              >
                <FontAwesomeIcon icon={faTableColumns} />
              </Button>
            </Show>
            <Show visible={combined}>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Full Page View"
                onClick={() => {
                  navigate(`${getContentPath(contentType)}${id}`);
                }}
              >
                <FontAwesomeIcon icon={faUpRightFromSquare} />
              </Button>
            </Show>
            <Show visible={!!id}>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Previous"
                onClick={() => {
                  const id = page?.items[indexPosition - 1]?.id;
                  if (!!id) {
                    if (!!combined) navigate(`/contents/combined/${id}`);
                    else navigate(`${getContentPath(contentType)}${id}`);
                  }
                }}
                disabled={!enablePrev}
              >
                <FaChevronLeft />
              </Button>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Next"
                onClick={() => {
                  const id = page?.items[indexPosition + 1]?.id;
                  if (combined) navigate(`/contents/combined/${id}`);
                  else navigate(`${getContentPath(contentType)}${id}`);
                }}
                disabled={!enableNext}
              >
                <FaChevronRight />
              </Button>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Reload"
                onClick={() => {
                  fetchContent(form.id);
                }}
              >
                <FaSpinner />
              </Button>
            </Show>
            <Row className="frm-in content-status">
              <label>Status:</label>
              <span>{getStatusText(form.status)}</span>
            </Row>
          </Row>
          <FormikForm
            onSubmit={handleSave}
            validationSchema={ContentFormSchema}
            initialValues={form}
            loading={(request: IAjaxRequest) =>
              !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
            }
          >
            {(props) => (
              <Col>
                <ContentFormToolBar
                  contentType={contentType}
                  status={getStatusText(form.status)}
                  determineActions={determineActions()}
                />

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
                          <Row>
                            <FormikSelect
                              name="sourceId"
                              label="Source"
                              width={FieldSize.Big}
                              value={
                                sourceOptions.find((mt) => mt.value === props.values.sourceId) ?? ''
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
                              options={filterEnabled(sourceOptions, props.values.sourceId).filter(
                                (x) =>
                                  contentType !== ContentTypeName.Image ||
                                  x.label.includes('(TC)') ||
                                  x.label.includes('(PROVINCE)') ||
                                  x.label.includes('(GLOBE)') ||
                                  x.label.includes('(POST)') ||
                                  x.label.includes('(SUN)'),
                              )}
                              required={
                                !props.values.otherSource || props.values.otherSource !== ''
                              }
                              isDisabled={!!props.values.tempSource}
                            />
                            <FormikSelect
                              name="productId"
                              value={
                                productOptions.find((mt) => mt.value === props.values.productId) ??
                                ''
                              }
                              label="Product"
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
                        </Col>
                      </Row>
                      <Row>
                        <Col grow={1}></Col>
                      </Row>
                      <Show visible={contentType === ContentTypeName.PrintContent}>
                        <Row>
                          <FormikText name="byline" label="Byline" required />
                          <FormikText name="edition" label="Edition" />
                          <FormikText name="section" label="Section" required />
                          <FormikText name="page" label="Page" />
                        </Row>
                      </Show>
                      <Show
                        visible={
                          contentType === ContentTypeName.Snippet ||
                          contentType === ContentTypeName.Story
                        }
                      >
                        <Row>
                          <FormikText name="byline" label="Byline" />
                          <FormikText
                            style={{ textDecoration: textDecorationStyle, cursor: cursorStyle }}
                            className="source-url"
                            name="sourceUrl"
                            label="Source URL"
                            tooltip="The URL to the original source story"
                            width={FieldSize.Large}
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
                        </Row>
                      </Show>
                    </Col>
                  </Show>
                </Row>
                <Row>
                  <Show visible={contentType !== ContentTypeName.PrintContent}>
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
                          <Show visible={props.values.contentType === ContentTypeName.Snippet}>
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
                            <Tab
                              label="Clips"
                              onClick={() => setActive('clips')}
                              active={active === 'clips'}
                              hasErrors={!!clipErrors && active !== 'clips'}
                              showErrorOnSave={{ value: true, savePressed: savePressed }}
                            />
                          </Show>
                          <Show visible={props.values.contentType !== ContentTypeName.Image}>
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
                          </Show>
                        </>
                      }
                    >
                      <Show visible={active === 'properties'}>
                        <ContentSummaryForm
                          content={form}
                          setContent={setForm}
                          contentType={contentType}
                          savePressed={savePressed}
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
                  <Show visible={contentType === ContentTypeName.PrintContent}>
                    <ContentSummaryForm
                      content={form}
                      setContent={setForm}
                      contentType={contentType}
                    />
                  </Show>
                </Row>
                <Row className="submit-buttons">
                  <Button
                    variant={ButtonVariant.success}
                    disabled={
                      props.isSubmitting ||
                      (contentType === ContentTypeName.Snippet &&
                        props.values.fileReferences.length === 0 &&
                        !props.values.file)
                    }
                    onClick={() => {
                      setSavePressed(true);
                      handlePublish(props);
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
                      variant={ButtonVariant.success}
                      disabled={
                        props.isSubmitting ||
                        (contentType === ContentTypeName.Snippet &&
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
                  <Button
                    type="submit"
                    disabled={props.isSubmitting}
                    onClick={() => setSavePressed(true)}
                  >
                    Save without publishing
                  </Button>
                  <Show visible={!!props.values.id}>
                    <Show
                      visible={
                        !!props.values.id && props.values.contentType === ContentTypeName.Snippet
                      }
                    >
                      <Button
                        onClick={() =>
                          isWorkOrderStatus(form.workOrders, WorkOrderTypeName.Transcription, [
                            WorkOrderStatusName.Completed,
                          ])
                            ? toggleTranscribe()
                            : handleTranscribe(props.values)
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

                    <Button
                      onClick={toggleDelete}
                      variant={ButtonVariant.danger}
                      disabled={props.isSubmitting}
                    >
                      Delete
                    </Button>
                  </Show>
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
                      await handleTranscribe(props.values);
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
                      await handleNLP(props.values);
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
