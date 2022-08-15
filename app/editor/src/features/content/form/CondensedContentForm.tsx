import { faUpRightFromSquare } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Area, IOptionItem } from 'components/form';
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
  ContentStatusName,
  ContentType,
  IContentModel,
  IUserModel,
  ValueType,
} from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import ReactTooltip from 'react-tooltip';
import { useContent, useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  Row,
  Show,
  Tab,
  Tabs,
  useKeycloakWrapper,
} from 'tno-core';
import { getDataSourceOptions, getSortableOptions } from 'utils';

import { ContentFormSchema } from '../validation';
import { ContentActions, ContentSummaryForm, ContentTranscriptForm } from '.';
import { defaultFormValues } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

export interface ICondensedContentFormProps {
  /** The content type this form will create */
  contentType?: ContentType;
  setUpdated: (updated: boolean) => void;
}

/**
 * Content Form edit and create form for default view. Path will be appended with content id.
 * @returns Edit/Create Form for Content
 */
export const CondensedContentForm: React.FC<ICondensedContentFormProps> = ({
  contentType = ContentType.Snippet,
  setUpdated,
}) => {
  const keycloak = useKeycloakWrapper();
  const navigate = useNavigate();
  const [otherSource, setOtherSource] = React.useState(false);
  const { id } = useParams();
  const [{ dataSources, mediaTypes, tonePools, users, series }, { getSeries, getUsers }] =
    useLookup();
  const [, { getContent, addContent, updateContent, deleteContent, upload }] = useContent();
  const { isShowing, toggle } = useModal();

  const [active, setActive] = React.useState('properties');
  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [dataSourceOptions, setDataSourceOptions] = React.useState<IOptionItem[]>([]);
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues,
    id: parseInt(id ?? '0'),
  });
  const [fetchingUsers, setFetchingUsers] = React.useState(false);

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id ?? 0;

  React.useEffect(() => {
    setDataSourceOptions(getDataSourceOptions(dataSources));
  }, [dataSources]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(mediaTypes));
  }, [mediaTypes]);

  React.useEffect(() => {
    if (!!id && +id > 0) {
      getContent(+id).then((data) => setContent(toForm(data)));
    }
  }, [id, getContent]);

  React.useEffect(() => {
    // If for some reason the current user does not exist in the local list, go fetch a new list from the api.
    if (!userId && !fetchingUsers) {
      setFetchingUsers(true);
      getUsers(true);
    }
  }, [fetchingUsers, getUsers, userId]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const handleSubmit = async (values: IContentForm) => {
    var contentResult: IContentModel | null = null;
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

      // Now upload the file if it exists.
      if (!!values.file) {
        const uploadResult = await upload(contentResult, values.file);
        setContent(toForm(uploadResult));
      } else {
        setContent(toForm(contentResult));
      }

      toast.success(`${contentResult.headline} has successfully been saved.`);
      if (!originalId) navigate(`/contents/${contentResult.id}`);
      if (!!contentResult?.seriesId) {
        // A dynamically added series has been added, fetch the latests series.
        const newSeries = series.find((s) => s.id === contentResult?.seriesId);
        if (!newSeries) getSeries();
      }
    } catch {
      // If the upload fails, we still need to update the form from the original update.
      if (!!contentResult) {
        setContent(toForm(contentResult));
        if (!originalId) navigate(`/contents/${contentResult.id}`);
      }
    } finally {
      setUpdated(true);
    }
  };

  return (
    <styled.ContentForm>
      <FormPage className="condensed">
        <Area>
          <FormikForm
            onSubmit={handleSubmit}
            validationSchema={ContentFormSchema}
            initialValues={content}
          >
            {(props) => (
              <Col>
                <Row alignItems="flex-start">
                  <Col flex="1 1 auto">
                    <FormikText
                      name="headline"
                      required
                      label="Headline"
                      value={props.values.headline}
                    />
                    <Row>
                      <FormikSelect
                        name="dataSourceId"
                        label="Source"
                        width={FieldSize.Big}
                        value={
                          dataSourceOptions.find((mt) => mt.value === props.values.dataSourceId) ??
                          ''
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
                    </Row>
                    <Row>
                      <FormikCheckbox
                        name="showOther"
                        label="Other Source"
                        checked={props.values.showOther}
                        onChange={(e: any) => {
                          if (e.target.checked) {
                            setOtherSource(true);
                          } else {
                            setOtherSource(!otherSource);
                          }
                          props.setFieldValue('showOther', e.target.checked);
                        }}
                      />
                    </Row>
                    <Show visible={otherSource}>
                      <Row>
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
                    </Show>
                    <Row>
                      <Col grow={1}>
                        <FormikSelect
                          name="mediaTypeId"
                          value={
                            mediaTypeOptions.find((mt) => mt.value === props.values.mediaTypeId) ??
                            ''
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
                    <FormikHidden name="uid" />
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
                  <Col className="checkbox-column" flex="1 1 auto">
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
                            e.target.checked ? ContentStatusName.Publish : ContentStatusName.Draft,
                          );
                        }}
                      />
                      {/* section one of actions */}
                      <Col alignSelf="stretch">
                        <ContentActions init filter={(a) => a.valueType === ValueType.Boolean} />
                      </Col>
                    </Col>
                    <Row className="commentary">
                      <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
                    </Row>
                  </Col>
                  <FontAwesomeIcon
                    onClick={() => navigate(`/contents/${id}`)}
                    size="lg"
                    className="popout"
                    icon={faUpRightFromSquare}
                  />
                </Row>
                <Row>
                  <Show visible={contentType === ContentType.Snippet}>
                    <Tabs
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
                        </>
                      }
                    >
                      {active === 'properties' ? (
                        <ContentSummaryForm
                          content={content}
                          setContent={setContent}
                          contentType={contentType}
                        />
                      ) : (
                        <ContentTranscriptForm />
                      )}
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
                <Row style={{ marginTop: '2%' }}>
                  <Button style={{ marginRight: '4%' }} type="submit" disabled={props.isSubmitting}>
                    {!id ? 'Create Snippet' : 'Update Snippet'}
                  </Button>
                  <Button onClick={toggle} variant={ButtonVariant.danger}>
                    Remove Snippet
                  </Button>
                </Row>
                <Modal
                  headerText="Confirm Removal"
                  body="Are you sure you wish to remove this snippet?"
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Remove It"
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
