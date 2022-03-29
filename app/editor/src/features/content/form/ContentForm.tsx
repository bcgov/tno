import { Button, ButtonVariant } from 'components/button';
import { Col } from 'components/flex/col';
import { Row } from 'components/flex/row';
import { Area, IOptionItem, OptionItem } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { Modal } from 'components/modal';
import { Tab, Tabs } from 'components/tabs';
import { Formik } from 'formik';
import { IUserModel, ActionName, ContentStatus } from 'hooks/api-editor';
import { useKeycloakWrapper } from 'hooks';
import useModal from 'hooks/modal/useModal';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useContent, useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

import { PropertiesContentForm } from '.';
import { ActionCheckbox } from './ActionCheckbox';
import { defaultFormValues } from './constants';
import { IContentForm } from './interfaces';
import { TranscriptContentForm } from './TranscriptContentForm';
import { toForm, toModel } from './utils';

/**
 * Content Form edit and create form for default view. Path will be appended with content id.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const [{ mediaTypes, users }] = useLookup();
  const [, { getContent, addContent, updateContent, deleteContent }] = useContent();
  const { isShowing, toggle } = useModal();

  const [active, setActive] = React.useState('properties');
  const [mediaTypeOptions, setMediaTypeOptions] = React.useState<IOptionItem[]>([]);
  const [content, setContent] = React.useState<IContentForm>({
    ...defaultFormValues,
    id: parseInt(id ?? '0'),
  });
  const [toggleCommentary, setToggleCommentary] = React.useState(true);
  const keycloak = useKeycloakWrapper();

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

  // include id when it is an update, no idea necessary when new content
  const submitContent = async (values: IContentForm) => {
    values.contentTypeId = 1;
    values.ownerId = !content.id ? userId ?? 0 : values.ownerId;
    const model = toModel(values);
    const result = !content.id ? await addContent(model) : await updateContent(model);
    toForm(result);
  };

  React.useEffect(() => {
    if (!!content.id) {
      getContent(content.id).then((data) => setContent(toForm(data)));
    }
  }, [content.id, getContent]);

  React.useEffect(() => {
    setMediaTypeOptions(getSortableOptions(mediaTypes, [new OptionItem<number>('All Media', 0)]));
  }, [mediaTypes]);

  return (
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
      <Formik
        enableReinitialize
        onSubmit={async (values) => {
          try {
            await submitContent(values);
          } finally {
            navigate('/contents');
          }
        }}
        initialValues={content}
      >
        {(props) => (
          <Col>
            <Row style={{ marginTop: '2%' }}>
              <Col>
                <Row>
                  <FormikText
                    className="lrg"
                    name="headline"
                    label="Headline"
                    value={props.values.headline}
                    onChange={props.handleChange}
                  />
                </Row>
                <Row>
                  <Col>
                    <FormikText
                      className="md"
                      name="source"
                      label="Source"
                      value={props.values.source}
                      onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                        props.setFieldValue('source', e.target.value)
                      }
                    />
                  </Col>
                  <Col style={{ marginLeft: '5%' }}>
                    <FormikText disabled className="md" name="otherSource" label="Other Source" />
                  </Col>
                </Row>
                <Row>
                  <FormikSelect
                    className="md"
                    name="mediaTypeId"
                    value={mediaTypeOptions.find((mt) => mt.value === props.values.mediaTypeId)}
                    onChange={(e: any) => {
                      props.setFieldValue('mediaTypeId', e.value);
                    }}
                    label="Media Type"
                    options={mediaTypeOptions}
                  />
                </Row>
              </Col>
              <Col style={{ marginLeft: '3%', marginRight: '10%' }}>
                <Row style={{ marginTop: '4.5%' }}>
                  <Col style={{ width: '215px' }}>
                    <FormikCheckbox disabled className="chk" name="publish" label="Publish" />
                    <FormikCheckbox disabled className="chk" name="alert" label="Alert" />
                    <FormikCheckbox className="chk" name="frontPage" disabled label="Front Page" />
                    <FormikCheckbox
                      className="chk"
                      name="publish"
                      label="Publish"
                      checked={
                        props.values.status === ContentStatus.Publish ||
                        props.values.status === ContentStatus.Published
                      }
                      onChange={(e: any) => {
                        props.setFieldValue(
                          'status',
                          e.target.checked ? ContentStatus.Publish : ContentStatus.Draft,
                        );
                      }}
                    />
                    <ActionCheckbox name={ActionName.Alert} />
                    <ActionCheckbox name={ActionName.FrontPage} />
                    <ActionCheckbox
                      name={ActionName.Commentary}
                      onClick={() => {
                        setToggleCommentary(!toggleCommentary);
                      }}
                    />
                  </Col>
                  <Col style={{ width: '215px' }}>
                    <ActionCheckbox name={ActionName.TopStory} />
                    <ActionCheckbox name={ActionName.OnTicker} />
                    <ActionCheckbox name={ActionName.NonQualified} />
                  </Col>
                </Row>
                <Row>
                  <FormikText
                    disabled={toggleCommentary}
                    name="timeout"
                    label="Commentary Timeout"
                    className="md"
                  />
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
              <Button
                style={{ marginRight: '4%' }}
                type="submit"
                onClick={async () => {
                  try {
                    await submitContent(props.values);
                  } finally {
                    navigate('/contents');
                  }
                }}
              >
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
      </Formik>
    </Area>
  );
};
