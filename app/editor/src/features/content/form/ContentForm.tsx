import {
  Area,
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikDropdown,
  FormikText,
  IOptionItem,
  OptionItem,
  Row,
  Tab,
  TabContainer,
} from 'components';
import { Formik } from 'formik';
import { IContentApi, useApiEditor } from 'hooks';
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLookup } from 'store';

import { PropertiesContentForm } from '.';
import { formContentToApiContent } from './contentFormUtils';
import { TranscriptContentForm } from './TranscriptContentForm';

/**
 * Content Form edit and create form for default view. Path will be appeneded with content id.
 * @returns Edit/Create Form for Content
 */
export const ContentForm: React.FC = () => {
  const [active, setActive] = useState('properties');
  const navigate = useNavigate();
  const {
    storeMediaTypes,
    state: { mediaTypes },
  } = useLookup();
  const [mediaTypeOptions, setMediatTypeOptions] = useState<IOptionItem[]>([]);
  const api = useApiEditor({ baseURL: `/api/editor/contents/` });
  const lookupApi = useApiEditor({ baseURL: '/api' });
  const path = window.location.pathname.toString();
  const getContentId = () => {
    return Number(path.substring(path.lastIndexOf('/') + 1));
  };

  const [mode] = useState<'edit' | 'create'>(getContentId() === 0 ? 'create' : 'edit');

  const defaultValues: IContentApi = {
    id: 0,
    status: 0,
    mediaTypeId: 0,
    headline: '',
    summary: '',
    contentTypeId: 0,
    source: '',
    licenseId: 0,
    page: '',
    section: '',
    transcription: '',
    ownerId: 0,
    createdOn: undefined,
    seriesId: undefined,
  };
  const id = getContentId();
  const [content, setContent] = useState<IContentApi>(defaultValues);
  const [toggleCommentary, setToggleCommentary] = useState(true);

  // include id when it is an update, no idea necessary when new content
  const submitContent = async (data: IContentApi, contentId?: number) => {
    mode === 'create' ? await api.addContent(data) : await api.updateContent(data, id);
    navigate('/contents');
  };

  useEffect(() => {
    api.findContent(id).then((data) => setContent(data));
  }, [api, id]);

  // Populate lookup values on initial render
  useEffect(() => {
    mediaTypes.length === 0 && lookupApi.getMediaTypes().then((data) => storeMediaTypes(data));
  }, [lookupApi, mediaTypes, storeMediaTypes]);

  useEffect(() => {
    setMediatTypeOptions(
      [new OptionItem('All Media', 0)].concat(mediaTypes.map((m) => new OptionItem(m.name, m.id))),
    );
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
        onSubmit={(values) => {
          submitContent(formContentToApiContent(values));
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
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                      setContent({ ...content, headline: e.target.value })
                    }
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
                        setContent({ ...content, source: e.target.value })
                      }
                    />
                  </Col>
                  <Col style={{ marginLeft: '5%' }}>
                    <FormikText disabled className="md" name="otherSource" label="Other Source" />
                  </Col>
                </Row>
                <Row>
                  <FormikDropdown
                    className="md"
                    name="mediaTypeId"
                    value={mediaTypeOptions.find((mt) => mt.value === content.mediaTypeId)}
                    onChange={(e: any) => setContent({ ...content, mediaTypeId: e.value })}
                    label="Media Type"
                    options={mediaTypeOptions}
                  />
                </Row>
              </Col>
              <Col style={{ marginLeft: '3%', marginRight: '10%' }}>
                <Row style={{ marginTop: '4.5%' }}>
                  <Col style={{ width: '215px' }}>
                    <FormikCheckbox
                      disabled
                      className="chk"
                      name="publish"
                      labelRight
                      label="Publish"
                    />
                    <FormikCheckbox
                      disabled
                      className="chk"
                      name="alert"
                      labelRight
                      label="Alert"
                    />
                    <FormikCheckbox
                      className="chk"
                      name="frontPage"
                      labelRight
                      disabled
                      label="Front Page"
                    />
                    <FormikCheckbox
                      name="commentary"
                      className="chk"
                      disabled={mode === 'create'}
                      onClick={() => setToggleCommentary(!toggleCommentary)}
                      labelRight
                      label="Commentary"
                    />
                  </Col>
                  <Col style={{ width: '215px' }}>
                    <FormikCheckbox
                      disabled
                      className="chk"
                      name="topStory"
                      labelRight
                      label="Top Story "
                    />
                    <FormikCheckbox
                      disabled
                      className="chk"
                      name="onTicker"
                      labelRight
                      label="On Ticker"
                    />
                    <FormikCheckbox
                      className="chk"
                      name="nonQualified"
                      disabled
                      labelRight
                      label="Non Qualified Subject"
                    />
                  </Col>
                </Row>
                <Row>
                  <FormikText
                    name="timeout"
                    value="NON FUNCTIONAL"
                    label="Commentary Timeout"
                    disabled={mode === 'edit' ? toggleCommentary : true}
                    className="md"
                  />
                </Row>
              </Col>
            </Row>
            <Row>
              <TabContainer
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
                  <TranscriptContentForm content={content} setContent={setContent} />
                )}
              </TabContainer>
            </Row>
            <Row style={{ marginTop: '2%' }}>
              <Button
                style={{ marginRight: '4%' }}
                type="submit"
                disabled={mode === 'edit'}
                onClick={() => submitContent(formContentToApiContent(content))}
              >
                {mode === 'create' ? 'Create Snippet' : 'Update Snippet'}
              </Button>
              <Button disabled variant={ButtonVariant.danger}>
                Remove Snippet{' '}
              </Button>
            </Row>
          </Col>
        )}
      </Formik>
    </Area>
  );
};
