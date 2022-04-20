import { Area, FieldSize, IOptionItem } from 'components/form';
import { FormPage } from 'components/form/formpage';
import {
  FormikCheckbox,
  FormikForm,
  FormikHidden,
  FormikSelect,
  FormikText,
} from 'components/formik';
import { Modal } from 'components/modal';
import { ContentStatusName, IUserModel, ValueType } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonVariant, Col, Row, Tab, Tabs, useKeycloakWrapper } from 'tno-core';
import { getDataSourceOptions, getSortableOptions } from 'utils';

import { ContentFormSchema } from '../validation';
import { ContentActions, PropertiesContentForm } from '.';
import { defaultFormValues } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { TranscriptContentForm } from './TranscriptContentForm';
import { toForm, toModel } from './utils';

/**
 * Content Form edit and create form for default view. Path will be appended with content id.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ dataSources, mediaTypes, tonePools, users, series }, { getSeries }] = useLookup();
  const [, { getContent, addContent, updateContent, deleteContent }] = useContent();
  const { isShowing, toggle } = useModal();

  const [active, setActive] = React.useState('properties');
  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [dataSourceOptions, setDataSourceOptions] = React.useState<IOptionItem[]>([]);
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues,
    id: parseInt(id ?? '0'),
  });
  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

  React.useEffect(() => {
    setDataSourceOptions(getDataSourceOptions(dataSources));
  }, [dataSources]);

  React.useEffect(() => {
    if (!!content.id) {
      getContent(content.id).then((data) => setContent(toForm(data)));
    }
  }, [content.id, getContent]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(mediaTypes));
  }, [mediaTypes]);

  const handleSubmit = async (values: IContentForm) => {
    const originalId = values.id;
    values.contentTypeId = 1; // TODO: This should be based on some logic.
    values.ownerId = !content.id ? userId ?? 0 : values.ownerId;
    const model = toModel(
      values,
      tonePools.find((t) => t.name === 'Default'),
    );
    const result = !content.id ? await addContent(model) : await updateContent(model);
    setContent(toForm(result));
    toast.success(`${result.headline} has successfully been saved.`);

    if (!!originalId) navigate(`/contents/${result.id}`);
    if (!!result.seriesId) {
      // A dynamically added series has been added, fetch the latests series.
      const newSeries = series.find((s) => s.id === result.seriesId);
      if (!newSeries) getSeries();
    }
  };

  return (
    <styled.ContentForm>
      <FormPage height="fit-content">
        <Area>
          <Row>
            <Button variant={ButtonVariant.action} onClick={() => navigate('/contents')}>
              <Row>
                <img
                  style={{ marginRight: '0.5em' }}
                  alt="back"
                  src={process.env.PUBLIC_URL + '/assets/back_arrow.svg'}
                />
                <p>Back to List View</p>
              </Row>
            </Button>
          </Row>
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
                      <Col>
                        <FormikSelect
                          name="dataSourceId"
                          label="Source"
                          value={
                            dataSourceOptions.find(
                              (mt) => mt.value === props.values.dataSourceId,
                            ) ?? ''
                          }
                          onChange={(e: any) => {
                            props.setFieldValue('dataSourceId', e.value);
                            props.setFieldValue('source', e.label);
                          }}
                          width={FieldSize.Medium}
                          options={dataSourceOptions}
                          required={!props.values.source}
                          isDisabled={!!props.values.otherSource}
                        />
                        <FormikHidden name="source" />
                      </Col>
                      <Col>
                        <FormikText
                          width={FieldSize.Medium}
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
                      </Col>
                    </Row>
                    <FormikSelect
                      name="mediaTypeId"
                      value={
                        mediaTypeOptions.find((mt) => mt.value === props.values.mediaTypeId) ?? ''
                      }
                      label="Media Type"
                      options={mediaTypeOptions}
                      required
                    />
                    <FormikText
                      name="sourceURL"
                      label="Source URL"
                      tooltip="The URL to the original source story"
                      onChange={(e) => {
                        props.handleChange(e);
                        if (!!props.values.uid && !!e.currentTarget.value)
                          props.setFieldValue('uid', e.currentTarget.value);
                        else props.setFieldValue('uid', '');
                      }}
                    />
                    <FormikHidden name="uid" />
                  </Col>
                  <Col className="checkbox-column" flex="1 1 auto">
                    <Col style={{ marginTop: '4.5%' }} alignItems="flex-start" wrap="wrap">
                      <FormikCheckbox
                        className="chk"
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
                      <ContentActions init filter={(a) => a.valueType === ValueType.Boolean} />
                    </Col>
                    <Row>
                      <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
                    </Row>
                  </Col>
                </Row>
                <Row>
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
                      <PropertiesContentForm content={content} setContent={setContent} />
                    ) : (
                      <TranscriptContentForm />
                    )}
                  </Tabs>
                </Row>
                <Row style={{ marginTop: '2%' }}>
                  <Button style={{ marginRight: '4%' }} type="submit" disabled={props.isSubmitting}>
                    {!content.id ? 'Create Snippet' : 'Update Snippet'}
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
