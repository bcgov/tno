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
import {
  useCombinedView,
  useDataSourceOptions,
  useMediaTypeOptions,
  useTooltips,
  useUserLookups,
} from 'hooks';
import { ContentStatusName, ContentType, IContentModel, ValueType } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { FaBars, FaChevronLeft, FaChevronRight, FaGripLines, FaSpinner } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonVariant, Col, FieldSize, Row, Show, Tab, Tabs } from 'tno-core';

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
import { switchStatus, toForm, toModel } from './utils';

export interface IContentFormProps {
  /** The content type this form will create */
  contentType?: ContentType;
}

/**
 * Snippet Form edit and create form for default view. Path will be appended with content id.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC<IContentFormProps> = ({ contentType = ContentType.Snippet }) => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ dataSources, tonePools, series }, { getSeries }] = useLookup();
  const [
    { content: page },
    { getContent, addContent, updateContent, deleteContent, upload, publishContent, attach },
  ] = useContent();
  const { isShowing, toggle } = useModal();
  const { userId } = useUserLookups();
  const dataSourceOptions = useDataSourceOptions();
  const mediaTypeOptions = useMediaTypeOptions();
  const combined = useCombinedView();
  useTooltips();

  const [size, setSize] = React.useState(1);
  const [active, setActive] = React.useState('properties');
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues,
    id: parseInt(id ?? '0'),
  });

  const indexPosition = !!id ? page?.items.findIndex((c) => c.id === +id) ?? -1 : -1;
  const enablePrev = indexPosition > 0;
  const enableNext = indexPosition < (page?.items.length ?? 0) - 1;

  const fetchContent = React.useCallback(
    (id: number) => {
      getContent(id).then((data) => {
        setContent(toForm(data));
      });
    },
    [getContent],
  );

  React.useEffect(() => {
    if (!!id && +id > 0) {
      fetchContent(+id);
    }
  }, [id, fetchContent]);

  const handleSubmit = async (values: IContentForm) => {
    let contentResult: IContentModel | null = null;
    const originalId = values.id;
    try {
      if (!values.id) {
        // Only new content is initialized.
        values.contentTypeId = contentType;
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

      toast.success(`${contentResult.headline} has successfully been saved.`);

      if (!originalId) navigate(`/contents/${combined ? 'combined/' : ''}${contentResult.id}`);
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

  const handlePublish = async (values: IContentForm) => {
    const defaultTonePool = tonePools.find((t) => t.name === 'Default');
    values.tonePools = !!defaultTonePool ? [{ ...defaultTonePool, value: +values.tone }] : [];

    const model = toModel(values);
    const result = await publishContent(model);
    setContent(toForm(result));
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
                  navigate(`/snippets/${id}`);
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
                  if (!!id) navigate(`/contents/${combined ? 'combined/' : ''}${id}`);
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
                  if (!!id) navigate(`/contents/${combined ? 'combined/' : ''}${id}`);
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
                        <FormikText
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
                          <FormikText
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
                            tooltip="Minimize Details"
                            onClick={() => setSize(0)}
                          >
                            <FaGripLines />
                          </Button>
                        </Col>
                      </Row>
                      <Row>
                        <FormikSelect
                          name="dataSourceId"
                          label="Source"
                          width={FieldSize.Big}
                          value={
                            dataSourceOptions.find(
                              (mt) => mt.value === props.values.dataSourceId,
                            ) ?? ''
                          }
                          onChange={(newValue: any) => {
                            const dataSource = dataSources.find((ds) => ds.id === newValue.value);
                            props.setFieldValue('dataSourceId', newValue.value);
                            props.setFieldValue('source', dataSource?.code ?? '');
                            if (!!dataSource) {
                              props.setFieldValue('mediaTypeId', dataSource.mediaTypeId);
                              props.setFieldValue('licenseId', dataSource.licenseId);
                            }
                          }}
                          options={dataSourceOptions}
                          required={!props.values.source}
                          isDisabled={!!props.values.otherSource}
                        />
                        <FormikHidden name="source" />
                        <FormikText
                          name="otherSource"
                          label="Other Source"
                          onChange={(e) => {
                            const value = e.currentTarget.value;
                            props.setFieldValue('otherSource', value);
                            props.setFieldValue('source', value);
                            if (!!value) {
                              props.setFieldValue('dataSourceId', undefined);
                            }
                          }}
                          required={!!props.values.otherSource}
                        />
                      </Row>
                      <Row>
                        <Col grow={1}>
                          <FormikSelect
                            name="mediaTypeId"
                            value={
                              mediaTypeOptions.find(
                                (mt) => mt.value === props.values.mediaTypeId,
                              ) ?? ''
                            }
                            label="Media Type"
                            options={mediaTypeOptions}
                            required
                          />
                        </Col>
                        <Show visible={contentType === ContentType.Print}>
                          <Col grow={1}>
                            <FormikText name="edition" label="Edition" />
                          </Col>
                        </Show>
                      </Row>
                      <Show visible={contentType === ContentType.Print}>
                        <Row>
                          <FormikText name="section" label="Section" required />
                          <FormikText name="storyType" label="Story Type" required />
                          <FormikText name="page" label="Page" />
                        </Row>
                        <FormikTextArea name="byline" label="By Line" required />
                      </Show>
                      <Show visible={contentType === ContentType.Snippet}>
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
                        <ContentActions init filter={(a) => a.valueType === ValueType.Boolean} />
                      </Col>
                      <Row className="commentary">
                        <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
                      </Row>
                    </Col>
                  </Show>
                </Row>
                <Row>
                  <Show visible={contentType === ContentType.Snippet}>
                    <Tabs
                      className={`tabs ${size === 1 ? 'small' : 'large'}`}
                      tabs={
                        <>
                          <Tab
                            label="Properties"
                            onClick={() => setActive('properties')}
                            active={active === 'properties'}
                          />
                          <Tab
                            label="Transcript"
                            onClick={() => setActive('transcript')}
                            active={active === 'transcript'}
                          />
                          <Tab
                            label="Clips"
                            onClick={() => setActive('clips')}
                            active={active === 'clips'}
                          />
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
                        />
                      </Show>
                      <Show visible={active === 'transcript'}>
                        <ContentTranscriptForm />
                      </Show>
                      <Show visible={active === 'clips'}>
                        <ContentClipForm content={content} setContent={setContent} />
                      </Show>
                      <Show visible={active === 'labels'}>
                        <ContentLabelsForm />
                      </Show>
                    </Tabs>
                  </Show>
                  <Show visible={contentType === ContentType.Print}>
                    <ContentSummaryForm
                      content={content}
                      setContent={setContent}
                      contentType={contentType}
                    />
                  </Show>
                </Row>
                <Row>
                  <Button type="submit" disabled={props.isSubmitting}>
                    Save
                  </Button>
                  <Show visible={!!props.values.id}>
                    <Button
                      onClick={() => handlePublish(props.values)}
                      variant={ButtonVariant.success}
                      disabled={
                        props.isSubmitting ||
                        (props.values.status !== ContentStatusName.Publish &&
                          props.values.status !== ContentStatusName.Published)
                      }
                    >
                      Publish
                    </Button>
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
