import { FormikForm } from 'components/formik';
import React from 'react';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useSources } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  hasErrors,
  IconButton,
  Modal,
  Row,
  Show,
  SourceSchema,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultSource } from './constants';
import { ISourceForm } from './interfaces';
import * as styled from './styled';
import { toForm, toModel } from './utils';

interface ISourceProps {}

const SourceForm: React.FC<ISourceProps> = (props) => {
  const [, api] = useSources();
  const { state } = useLocation();
  const { id } = useParams();
  const { isShowing, toggle } = useModal();

  const sourceId = Number(id);
  const [source, setSource] = React.useState<ISourceForm>((state as any)?.source ?? defaultSource);
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!!sourceId && source?.id !== sourceId) {
      setSource({ ...defaultSource, id: sourceId }); // Do this to stop double fetch.
      api.getSource(sourceId).then((data) => {
        setSource(toForm(data));
      });
    }
  }, [api, source?.id, sourceId]);

  const handleSubmit = async (values: ISourceForm) => {
    try {
      const originalId = values.id;
      const model = toModel(values);
      const result = !values.id ? await api.addSource(model) : await api.updateSource(model);
      setSource(toForm(result));
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/sources/${result.id}`);
    } catch {}
  };

  return (
    <styled.SourceForm>
      <Row>
        <IconButton
          iconType="back"
          label="Back to Sources"
          className="back-button"
          onClick={() => navigate('/admin/sources')}
        />
        <Col flex="1" className="info">
          <p>Sources provide a way to identify and organize content.</p>
        </Col>
      </Row>
      <FormikForm
        initialValues={source}
        validationSchema={SourceSchema}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
        validateOnBlur={true}
        validateOnChange={false}
        validateOnMount={false}
      >
        {({ isSubmitting, errors, values }) => (
          <Row>
            <Col flex="2 1">
              <Tabs
                tabs={
                  <>
                    <Tab
                      navigateTo="details"
                      label="Details"
                      exact
                      activePaths={[`${id}`]}
                      hasErrors={hasErrors(errors, ['name', 'code', 'licenseId'])}
                    />
                    <Tab navigateTo="metrics" label="Reach/Earned Media" />
                  </>
                }
              >
                <Outlet />
                <Row justifyContent="flex-end" className="form-actions">
                  <Button type="submit" disabled={isSubmitting}>
                    Save
                  </Button>
                  <Show visible={!!values.id}>
                    <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                      Delete
                    </Button>
                  </Show>
                </Row>
                <Modal
                  headerText="Confirm Removal"
                  body="Are you sure you wish to remove this source?"
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Remove It"
                  onConfirm={async () => {
                    try {
                      await api.deleteSource(toModel(source));
                      toast.success(`${source.name} has successfully been deleted.`);
                      navigate('/admin/sources');
                    } finally {
                      toggle();
                    }
                  }}
                />
              </Tabs>
            </Col>
          </Row>
        )}
      </FormikForm>
    </styled.SourceForm>
  );
};

export default SourceForm;
