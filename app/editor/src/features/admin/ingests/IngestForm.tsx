import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import { IngestSchema } from 'hooks/api-editor/validation';
import React from 'react';
import { FaEye, FaEyeSlash, FaSpinner } from 'react-icons/fa';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useIngests } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, IconButton, Row, Show, Tab, Tabs } from 'tno-core';

import { IngestStatus } from '.';
import { defaultIngest } from './constants';
import * as styled from './styled';

interface IIngestProps {}

export const IngestForm: React.FC<IIngestProps> = (props) => {
  const [, api] = useIngests();
  const { state } = useLocation();
  const { id } = useParams();
  const { isShowing, toggle } = useModal();

  const ingestId = Number(id);
  const [showStatus, setShowStatus] = React.useState<boolean>(true);
  const [ingest, setIngest] = React.useState<IIngestModel>((state as any)?.ingest ?? defaultIngest);
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!!ingestId && ingest?.id !== ingestId) {
      api.getIngest(ingestId).then((data) => {
        setIngest({ ...data });
      });
    }
  }, [api, ingest?.id, ingestId]);

  const handleSubmit = async (values: IIngestModel) => {
    try {
      const originalId = values.id;
      const model: IIngestModel = {
        ...values,
      };
      const result = !values.id ? await api.addIngest(model) : await api.updateIngest(model);
      setIngest({ ...result });
      api.findAllIngests();
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/ingests/${result.id}`);
    } catch {}
  };

  const reload = async () => {
    try {
      const data = await api.getIngest(ingestId);
      setIngest({ ...data });
    } catch {}
  };

  const hasErrors = (errors: any, props: string[]) => {
    return props.some((p) => !!errors[p]);
  };

  return (
    <styled.IngestForm>
      <Row>
        <IconButton
          iconType="back"
          label="Back to Ingests"
          className="back-button"
          onClick={() => navigate('/admin/ingests')}
        />
        <Col flex="1" className="info">
          <p>This provides a way to configure ingestion services.</p>
        </Col>
        <Button onClick={() => setShowStatus(!showStatus)} tooltip={'Toggle status information'}>
          {showStatus ? <FaEyeSlash /> : <FaEye />}
        </Button>
        <Button variant={ButtonVariant.secondary} tooltip="Reload" onClick={() => reload()}>
          <FaSpinner />
        </Button>
      </Row>
      <FormikForm
        initialValues={ingest}
        validationSchema={IngestSchema}
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
              <Show visible={showStatus}>
                <IngestStatus className="status" flex="1 1" />
              </Show>
              <Tabs
                tabs={
                  <>
                    <Tab
                      navigateTo="details"
                      label="Details"
                      exact
                      activePaths={[`${id}`]}
                      hasErrors={hasErrors(errors, ['name', 'topic', 'sourceId', 'productId'])}
                    />
                    <Tab
                      navigateTo="settings"
                      label="Settings"
                      hasErrors={hasErrors(errors, [
                        'ingestTypeId',
                        'sourceConnectionId',
                        'destinationConnectionId',
                        'configuration',
                      ])}
                    />
                    <Tab navigateTo="schedule" label="Schedule" />
                    <Tab navigateTo="ingesting" label="Ingesting" />
                  </>
                }
              >
                <Outlet />
                <Row justifyContent="flex-end" className="form-actions">
                  <Button type="submit" disabled={isSubmitting}>
                    Save
                  </Button>
                  {!!ingestId && (
                    <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                      Delete
                    </Button>
                  )}
                </Row>
                <Modal
                  headerText="Confirm Removal"
                  body="Are you sure you wish to remove this ingest?"
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Remove It"
                  onConfirm={async () => {
                    try {
                      await api.deleteIngest(ingest);
                      toast.success(`${ingest.name} has successfully been deleted.`);
                      navigate('/admin/ingests');
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
    </styled.IngestForm>
  );
};
