import { faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { faTableColumns } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Area, IconButton } from 'components/form';
import { FormPage } from 'components/form/formpage';
import {
  FormikCheckbox,
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
  useProductOptions,
  useSourceOptions,
  useTooltips,
  useUserLookups,
} from 'hooks';
import { ContentStatusName, IContentModel, ValueType } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import { useTabValidationToasts } from 'hooks/useTabValidationToasts';
import React from 'react';
import { FaBars, FaChevronLeft, FaChevronRight, FaGripLines, FaSpinner } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonVariant, Col, FieldSize, Row, Show, Tab, Tabs } from 'tno-core';
import { hasErrors } from 'utils';

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
import { isSnippetForm, switchStatus, toForm, toModel } from './utils';

export interface IContentFormProps {
  /** The content type this form will create */
  contentType: ContentTypeName;
}

/**
 * Snippet Form edit and create form for default view. Path will be appended with content id.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC<IContentFormProps> = ({ contentType: initContentType }) => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ sources, tonePools, series }, { getSeries }] = useLookup();
  const [
    { content: page },
    {
      getContent,
      addContent,
      updateContent,
      deleteContent,
      upload,
      publishContent,
      attach,
      transcribe,
      nlp,
    },
  ] = useContent();
  const { isShowing, toggle } = useModal();
  const { userId } = useUserLookups();
  const sourceOptions = useSourceOptions();
  const productOptions = useProductOptions();
  const combined = useCombinedView();
  useTooltips();

  const [contentType, setContentType] = React.useState(initContentType);
  const [size, setSize] = React.useState(1);
  const [active, setActive] = React.useState('properties');
  const [savePressed, setSavePressed] = React.useState(false);
  const [clipErrors, setClipErrors] = React.useState<string>('');
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
    id: parseInt(id ?? '0'),
  });

  const indexPosition = !!id ? page?.items.findIndex((c) => c.id === +id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (page?.items.length ?? 0) - 1;

  const determineActions = () => {
    if (contentType === ContentTypeName.Snippet)
      return (a: IActionModel) => a.valueType === ValueType.Boolean;
    if (contentType === ContentTypeName.PrintContent)
      return (a: IActionModel) =>
        a.valueType === ValueType.Boolean && a.name !== ActionName.NonQualified;
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
    }
  }, [id, fetchContent]);

  const { setShowValidationToast } = useTabValidationToasts();

  const handleSubmit = async (values: IContentForm) => {
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
          `${isSnippetForm(contentResult?.contentType) ? '/snippets/' : '/papers/'}${
            combined ? '/contents/combined/' : ''
          }${contentResult.id}`,
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
    const values = props.values;
    await props.validateForm(values);

    if (props.isValid) {
      const defaultTonePool = tonePools.find((t) => t.name === 'Default');
      values.tonePools = !!defaultTonePool ? [{ ...defaultTonePool, value: +values.tone }] : [];

      try {
        const model = toModel(values);
        const result = await publishContent(model);
        setContent(toForm(result));
        toast.success(
          `"${values.headline}" has successfully requested publishing and has been saved.`,
        );
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    }
  };

  const handleTranscribe = async (values: IContentForm) => {
    try {
      // Save before submitting request.
      await handleSubmit(values);
      await transcribe(toModel(values));

      toast.success(`"${values.headline}" has successfully requested a transcript`);
    } catch {
      // Ignore this failure it is handled by our global ajax requests.
    }
  };

  const handleNLP = async (values: IContentForm) => {
    try {
      // Save before submitting request.
      await handleSubmit(values);
      await nlp(toModel(values));

      toast.success(
        `"${values.headline}" has successfully requested a Natural Language Processing`,
      );
    } catch {
      // Ignore this failure it is handled by our global ajax requests.
    }
  };

  return (
    <styled.ContentForm className="content-form">
      <FormPage minWidth={combined ? '' : '1200px'}>
        <Area>
          <Row>
            <IconButton label="List View" onClick={() => navigate('/contents')} iconType="back" />
            <Show visible={!combined}>
              <Button
                variant={ButtonVariant.secondary}
                tooltip="Combined View"
                onClick={() => {
                  navigate(`/contents/combined/${id}`);
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
                  if (isSnippetForm(contentType)) navigate(`/snippets/${id}`);
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
          </Row>
          <FormikForm
            onSubmit={handleSubmit}
            validationSchema={ContentFormSchema}
            initialValues={content}
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
                          <Show visible={!isSnippetForm(contentType)}>
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
                            const source = sources.find((ds) => ds.id === newValue.value);
                            props.setFieldValue('sourceId', newValue.value);
                            props.setFieldValue('otherSource', source?.code ?? '');
                            if (!!source) {
                              props.setFieldValue('licenseId', source.licenseId);
                            }
                          }}
                          options={sourceOptions}
                          required={!props.values.otherSource}
                          isDisabled={!!props.values.tempSource}
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
                        <Show visible={!isSnippetForm(contentType)}>
                          <Col grow={1}>
                            <FormikText name="edition" label="Edition" />
                          </Col>
                        </Show>
                      </Row>
                      <Show visible={!isSnippetForm(contentType)}>
                        <Row>
                          <FormikText name="section" label="Section" required />
                          <FormikText name="page" label="Page" />
                        </Row>
                      </Show>
                      <Show visible={isSnippetForm(contentType)}>
                        <FormikText
                          name="sourceUrl"
                          label="Source URL"
                          tooltip="The URL to the original source story"
                          onChange={(e) => {
                            props.handleChange(e);
                            if (!!props.values.uid && !!e.currentTarget.value)
                              props.setFieldValue('uid', e.currentTarget.value);
                            else props.setFieldValue('uid', '');
                          }}
                        />
                      </Show>
                    </Col>
                    <Col className="checkbox-column" flex="0.5 1 0%">
                      <Col style={{ marginTop: '4.5%' }} alignItems="flex-start" wrap="wrap">
                        <FormikCheckbox
                          name="publish"
                          label="Publish"
                          checked={
                            props.values.status === ContentStatusName.Publish ||
                            props.values.status === ContentStatusName.Published
                          }
                          onChange={(e: any) => {
                            props.setFieldValue(
                              'status',
                              switchStatus(e.target.checked, props.values.status),
                            );
                          }}
                        />
                        <ContentActions
                          init
                          contentType={contentType}
                          filter={determineActions()}
                        />
                      </Col>
                      <Row className="commentary">
                        <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
                      </Row>
                    </Col>
                  </Show>
                </Row>
                <Row>
                  <Show visible={isSnippetForm(contentType)}>
                    <Tabs
                      className={`tabs ${size === 1 ? 'small' : 'large'}`}
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
                            />
                            <Tab
                              label="Clips"
                              onClick={() => setActive('clips')}
                              active={active === 'clips'}
                              hasErrors={!!clipErrors && active !== 'clips'}
                              showErrorOnSave={{ value: true, savePressed: savePressed }}
                            />
                          </Show>
                          <Tab
                            label="Labels"
                            onClick={() => setActive('labels')}
                            active={active === 'labels'}
                          />
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
                  <Show visible={!isSnippetForm(contentType)}>
                    <ContentSummaryForm
                      content={content}
                      setContent={setContent}
                      contentType={contentType}
                    />
                  </Show>
                </Row>
                <Row>
                  <Button
                    type="submit"
                    disabled={props.isSubmitting}
                    onClick={() => setSavePressed(true)}
                  >
                    Save
                  </Button>
                  <Show visible={!!props.values.id}>
                    <Button
                      onClick={() => handlePublish(props)}
                      variant={ButtonVariant.success}
                      disabled={props.isSubmitting}
                    >
                      Publish
                    </Button>
                    <Show
                      visible={
                        !!props.values.id && props.values.contentType === ContentTypeName.Snippet
                      }
                    >
                      <Button
                        onClick={() => handleTranscribe(props.values)}
                        variant={ButtonVariant.action}
                        disabled={
                          props.isSubmitting ||
                          !props.values.fileReferences.length ||
                          (props.values.fileReferences.length > 0 &&
                            !props.values.fileReferences[0].isUploaded)
                        }
                      >
                        Request Transcribe
                      </Button>
                    </Show>
                    <Show visible={!!props.values.id && props.values.body.length > 0}>
                      <Button
                        onClick={() => handleNLP(props.values)}
                        variant={ButtonVariant.action}
                        disabled={props.isSubmitting}
                      >
                        Request NLP
                      </Button>
                    </Show>
                    <Button
                      onClick={toggle}
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
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Delete It"
                  onConfirm={async () => {
                    try {
                      await deleteContent(toModel(props.values));
                    } finally {
                      toggle();
                      navigate('/contents');
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
