import { faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { faTableColumns } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Area, IconButton } from 'components/form';
import { FormPage } from 'components/form/formpage';
import {
  FormikForm,
  FormikHidden,
  FormikSelect,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { Modal } from 'components/modal';
import { FormikProps } from 'formik';
import {
  ActionName,
  ContentTypeName,
  IActionModel,
  useCombinedView,
  useLookupOptions,
  useTooltips,
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'hooks';
import { ContentStatusName, IContentModel, useApiHubConnection, ValueType } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import { useTabValidationToasts } from 'hooks/useTabValidationToasts';
import React from 'react';
import {
  FaArrowRight,
  FaBars,
  FaCheckCircle,
  FaChevronLeft,
  FaChevronRight,
  FaExclamationCircle,
  FaGripLines,
  FaSpinner,
  FaTimesCircle,
} from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useContent, useWorkOrders } from 'store/hooks';
import { IAjaxRequest } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  Row,
  Show,
  Spinner,
  SpinnerVariant,
  Tab,
  Tabs,
} from 'tno-core';
import { hasErrors } from 'utils';

import { getStatusText } from '../list-view/utils';
import { ContentFormSchema } from '../validation';
import {
  ContentActions,
  ContentClipForm,
  ContentLabelsForm,
  ContentSummaryForm,
  ContentTranscriptForm,
} from '.';
import { defaultFormValues } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import {
  isImageForm,
  isSnippetForm,
  isWorkOrderActive,
  isWorkOrderCancelled,
  isWorkOrderComplete,
  isWorkOrderFailed,
  toForm,
  toModel,
  triggerFormikValidate,
} from './utils';

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
  const [, { transcribe, nlp }] = useWorkOrders();
  const { isShowing: showDeleteModal, toggle: toggleDelete } = useModal();
  const { isShowing: showTranscribeModal, toggle: toggleTranscribe } = useModal();
  const { isShowing: showNLPModal, toggle: toggleNLP } = useModal();
  const [{ sources, tonePools, series, sourceOptions, productOptions }, { getSeries }] =
    useLookupOptions();
  const { combined, formType } = useCombinedView(initContentType);
  const { getConnection } = useApiHubConnection();
  useTooltips();

  const [contentType, setContentType] = React.useState(formType ?? initContentType);
  const [size, setSize] = React.useState(1);
  const [active, setActive] = React.useState('properties');
  const [savePressed, setSavePressed] = React.useState(false);
  const [clipErrors, setClipErrors] = React.useState<string>('');
  const [textDecorationStyle, setTextDecorationStyle] = React.useState('none');
  const [cursorStyle, setCursorStyle] = React.useState('text');
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
    id: parseInt(id ?? '0'),
  });

  const userId = userInfo?.id ?? '';
  const indexPosition = !!id ? page?.items.findIndex((c) => c.id === +id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (page?.items.length ?? 0) - 1;
  const isTranscribing = isWorkOrderActive(content, WorkOrderTypeName.Transcription);
  const isTranscribed = isWorkOrderComplete(content, WorkOrderTypeName.Transcription);
  const isTranscribeFailed = isWorkOrderFailed(content, WorkOrderTypeName.Transcription);
  const isTranscribeCancelled = isWorkOrderCancelled(content, WorkOrderTypeName.Transcription);
  const isNLPing = isWorkOrderActive(content, WorkOrderTypeName.NaturalLanguageProcess);
  const isNLPed = isWorkOrderComplete(content, WorkOrderTypeName.NaturalLanguageProcess);
  const isNLPFailed = isWorkOrderFailed(content, WorkOrderTypeName.NaturalLanguageProcess);
  const isNLPCancelled = isWorkOrderCancelled(content, WorkOrderTypeName.NaturalLanguageProcess);

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
        return (a: IActionModel) => a.valueType === ValueType.Boolean;
    }
  };

  const fetchContent = React.useCallback(
    (id: number) => {
      getContent(id).then((data) => {
        setContent(toForm(data));
        // If the form is loaded from the URL instead of clicking on the list view it defaults to the snippet form.
        setContentType(data.contentType);
      });
    },
    [getContent],
  );

  React.useEffect(() => {
    if (!!id && +id > 0) {
      fetchContent(+id);
      const connection = getConnection();

      connection
        .start()
        .then(() => {
          connection.on('Update', (contentId) => {
            if (contentId === +id) fetchContent(+id);
          });
        })
        .catch((error) => {
          console.error('signalr', error);
        });

      return function cleanUp() {
        connection.off('Update');
        connection.stop();
      };
    }
  }, [id, fetchContent, getConnection]);

  const { setShowValidationToast } = useTabValidationToasts();

  const handleSave = async (values: IContentForm) => {
    let contentResult: IContentModel | null = null;
    const originalId = values.id;
    try {
      if (!values.id) {
        // Only new content is initialized.
        values.contentType = contentType;
        values.ownerId = userId;
      }

      const defaultTonePool = tonePools.find((t) => t.name === 'Default');
      values.tonePools = !!defaultTonePool ? [{ ...defaultTonePool, value: +values.tone }] : [];

      const model = toModel(values);
      contentResult = !content.id ? await addContent(model) : await updateContent(model);

      if (!!values.file) {
        // TODO: Make it possible to upload on the initial save instead of a separate request.
        // Upload the file if one has been added.
        const result = await upload(contentResult, values.file);
        setContent(toForm({ ...result, tonePools: values.tonePools }));
      } else if (
        !originalId &&
        !!values.fileReferences.length &&
        !values.fileReferences[0].isUploaded
      ) {
        // TODO: Make it possible to upload on the initial save instead of a separate request.
        // A file was attached but hasn't been uploaded to the API.
        const fileReference = values.fileReferences[0];
        const result = await attach(contentResult.id, fileReference.path);
        setContent(toForm({ ...result, tonePools: values.tonePools }));
      } else {
        setContent(toForm({ ...contentResult, tonePools: values.tonePools }));
      }

      toast.success(`"${contentResult.headline}" has successfully been saved.`);

      if (!originalId)
        navigate(
          `${
            isImageForm(contentResult?.contentType)
              ? '/images/'
              : isSnippetForm(contentResult?.contentType)
              ? '/snippets/'
              : '/papers/'
          }${combined ? '/contents/combined/' : ''}${contentResult.id}`,
        );
      if (!!contentResult?.seriesId) {
        // A dynamically added series has been added, fetch the latests series.
        const newSeries = series.find((s) => s.id === contentResult?.seriesId);
        if (!newSeries) getSeries();
      }
    } catch {
      // If the upload fails, we still need to update the form from the original update.
      if (!!contentResult) {
        setContent(toForm(contentResult));
        if (!originalId) navigate(`/contents/${combined ? 'combined/' : ''}${contentResult.id}`);
      }
    }
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

  const handleTranscribe = async (values: IContentForm) => {
    try {
      // TODO: Only save when required.
      // Save before submitting request.
      await handleSave(values);
      const response = await transcribe(toModel(values));
      setContent({ ...content, workOrders: [response.data, ...content.workOrders] });

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
      await handleSave(values);
      const response = await nlp(toModel(values));
      setContent({ ...content, workOrders: [response.data, ...content.workOrders] });

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
            <IconButton label="List View" onClick={() => navigate('/contents')} iconType="back" />
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
                  if (isImageForm(contentType)) navigate(`/images/${id}`);
                  else if (isSnippetForm(contentType)) navigate(`/snippets/${id}`);
                  else navigate(`/papers/${id}`);
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
                    else if (isImageForm(contentType)) navigate(`/images/${id}`);
                    else if (isSnippetForm(contentType)) navigate(`/snippets/${id}`);
                    else navigate(`/papers/${id}`);
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
                  else if (isImageForm(contentType)) navigate(`/images/${id}`);
                  else if (isSnippetForm(contentType)) navigate(`/snippets/${id}`);
                  else navigate(`/papers/${id}`);
                }}
                disabled={!enableNext}
              >
                <FaChevronRight />
              </Button>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Reload"
                onClick={() => {
                  fetchContent(content.id);
                }}
              >
                <FaSpinner />
              </Button>
            </Show>
            <Row className="frm-in content-status">
              <label>Status:</label>
              <span>{getStatusText(content.status)}</span>
            </Row>
          </Row>
          <FormikForm
            onSubmit={handleSave}
            validationSchema={ContentFormSchema}
            initialValues={content}
            loading={(request: IAjaxRequest) =>
              !request.isSilent && request.group.some((g) => g === 'content' || g === 'lookup')
            }
          >
            {(props) => (
              <Col>
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
                            label="Headline"
                            value={props.values.headline}
                          />
                          <Show visible={!isSnippetForm(contentType) && !isImageForm(contentType)}>
                            <FormikText name="byline" label="Byline" required />
                          </Show>
                        </Col>
                        <Col>
                          <Button
                            className="minimize-details"
                            variant={ButtonVariant.link}
                            tooltip="Minimize Details"
                            onClick={() => setSize(0)}
                          >
                            <FaGripLines />
                          </Button>
                        </Col>
                      </Row>
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
                              if (!!source) {
                                props.setFieldValue('licenseId', source.licenseId);
                              }
                            }
                          }}
                          options={sourceOptions.filter(
                            (x) =>
                              !isImageForm(contentType) ||
                              x.label.includes('(TC)') ||
                              x.label.includes('(PROVINCE)') ||
                              x.label.includes('(GLOBE)') ||
                              x.label.includes('(POST)') ||
                              x.label.includes('(SUN)'),
                          )}
                          required={!props.values.otherSource}
                          isDisabled={!!props.values.tempSource}
                        />
                        <FormikHidden name="otherSource" />
                        <Show visible={!isImageForm(contentType)}>
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
                        </Show>
                      </Row>
                      <Row>
                        <Col grow={1}>
                          <FormikSelect
                            name="productId"
                            value={
                              productOptions.find((mt) => mt.value === props.values.productId) ?? ''
                            }
                            label="Designation"
                            options={productOptions}
                            required
                          />
                        </Col>
                        <Show visible={!isSnippetForm(contentType) && !isImageForm(contentType)}>
                          <Col grow={1}>
                            <FormikText name="edition" label="Edition" />
                          </Col>
                        </Show>
                      </Row>
                      <Show visible={!isSnippetForm(contentType) && !isImageForm(contentType)}>
                        <Row>
                          <FormikText name="section" label="Section" required />
                          <FormikText name="page" label="Page" />
                        </Row>
                      </Show>
                      <Show visible={isSnippetForm(contentType)}>
                        <FormikText
                          style={{ textDecoration: textDecorationStyle, cursor: cursorStyle }}
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
                          <Button
                            disabled={!props.values.sourceUrl}
                            variant={ButtonVariant.secondary}
                            onClick={() =>
                              window.open(props.values.sourceUrl, '_blank', 'noreferrer')
                            }
                          >
                            <FaArrowRight />
                          </Button>
                        </FormikText>
                      </Show>
                    </Col>
                    <Col className="checkbox-column" flex="0.5 1 0%">
                      <Col style={{ marginTop: '4.5%' }} alignItems="flex-start" wrap="wrap">
                        <ContentActions
                          init
                          contentType={contentType}
                          filter={determineActions()}
                        />
                      </Col>
                      <Show visible={!isImageForm(contentType)}>
                        <Row className="commentary">
                          <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
                        </Row>
                      </Show>
                    </Col>
                  </Show>
                </Row>
                <Row>
                  <Show visible={isSnippetForm(contentType) || isImageForm(contentType)}>
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
                              label="Transcript"
                              onClick={() => setActive('transcript')}
                              active={active === 'transcript'}
                            >
                              <Show visible={isTranscribing}>
                                <Spinner variant={SpinnerVariant.light} size="0.5em" />
                              </Show>
                              <Show visible={isTranscribed && !isTranscribing}>
                                <FaCheckCircle className="spinner" />
                              </Show>
                              <Show visible={isTranscribeFailed}>
                                <FaExclamationCircle className="spinner" />
                              </Show>
                              <Show visible={isTranscribeCancelled}>
                                <FaTimesCircle className="spinner" />
                              </Show>
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
                              <Show visible={isNLPing}>
                                <Spinner variant={SpinnerVariant.light} size="0.5em" />
                              </Show>
                              <Show visible={isNLPed && !isNLPing}>
                                <FaCheckCircle className="spinner" />
                              </Show>
                              <Show visible={isNLPFailed}>
                                <FaExclamationCircle className="spinner" />
                              </Show>
                              <Show visible={isNLPCancelled}>
                                <FaTimesCircle className="spinner" />
                              </Show>
                            </Tab>
                          </Show>
                        </>
                      }
                    >
                      <Show visible={active === 'properties'}>
                        <ContentSummaryForm
                          content={content}
                          setContent={setContent}
                          contentType={contentType}
                          savePressed={savePressed}
                        />
                      </Show>
                      <Show visible={active === 'transcript'}>
                        <ContentTranscriptForm />
                      </Show>
                      <Show visible={active === 'clips'}>
                        <ContentClipForm
                          content={content}
                          setContent={setContent}
                          setClipErrors={setClipErrors}
                        />
                      </Show>
                      <Show visible={active === 'labels'}>
                        <ContentLabelsForm />
                      </Show>
                    </Tabs>
                  </Show>
                  <Show visible={!isSnippetForm(contentType) && !isImageForm(contentType)}>
                    <ContentSummaryForm
                      content={content}
                      setContent={setContent}
                      contentType={contentType}
                    />
                  </Show>
                </Row>
                <Row className="submit-buttons">
                  <Button
                    type="submit"
                    disabled={props.isSubmitting}
                    onClick={() => setSavePressed(true)}
                  >
                    Save
                  </Button>
                  <Button
                    variant={ButtonVariant.success}
                    disabled={props.isSubmitting}
                    onClick={() => {
                      setSavePressed(true);
                      handlePublish(props);
                    }}
                  >
                    Publish
                  </Button>
                  <Show visible={!!props.values.id}>
                    <Show
                      visible={
                        !!props.values.id && props.values.contentType === ContentTypeName.Snippet
                      }
                    >
                      <Button
                        onClick={() =>
                          isTranscribed && !isTranscribing
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
                        tooltip="Request a transcription"
                      >
                        Transcribe
                      </Button>
                    </Show>
                    <Show visible={!!props.values.id && props.values.body.length > 0}>
                      <Button
                        onClick={() =>
                          isNLPed && !isNLPing ? toggleNLP() : handleNLP(props.values)
                        }
                        variant={ButtonVariant.action}
                        disabled={props.isSubmitting}
                        tooltip="Request Natural Language Processing"
                      >
                        NLP
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
