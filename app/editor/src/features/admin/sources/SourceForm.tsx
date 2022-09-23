import { IconButton } from 'components/form';
import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { ISourceModel } from 'hooks/api-editor';
import { SourceSchema } from 'hooks/api-editor/validation';
import React from 'react';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useSources } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, Row, Tab, Tabs } from 'tno-core';
import { hasErrors } from 'utils';

import { defaultSource } from './constants';
import * as styled from './styled';

interface ISourceProps {}

export const SourceForm: React.FC<ISourceProps> = (props) => {
  const [, api] = useSources();
  const { state } = useLocation();
  const { id } = useParams();
  const { isShowing, toggle } = useModal();
  const [, { getSources }] = useLookup();

  const sourceId = Number(id);
  const [source, setSource] = React.useState<ISourceModel>((state as any)?.source ?? defaultSource);
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!!sourceId && source?.id !== sourceId) {
      api.getSource(sourceId).then((data) => {
        setSource({ ...data });
      });
    }
  }, [api, source?.id, sourceId]);

  const handleSubmit = async (values: ISourceModel) => {
    try {
      const originalId = values.id;
      const model: ISourceModel = {
        ...values,
        ownerId: values.ownerId ? values.ownerId : undefined,
      };
      const result = !values.id ? await api.addSource(model) : await api.updateSource(model);
      setSource({ ...result });
      getSources();
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
        {({ isSubmitting, errors }) => (
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
                  {!!sourceId && (
                    <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                      Delete
                    </Button>
                  )}
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
                      await api.deleteSource(source);
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
