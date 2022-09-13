import { IconButton } from 'components/form';
import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IDataSourceModel } from 'hooks/api-editor';
import { DataSourceSchema } from 'hooks/api-editor/validation';
import React from 'react';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useDataSources } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, Row, Tab, Tabs } from 'tno-core';

import { DataSourceStatus } from '.';
import { defaultDataSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSourceForm: React.FC<IDataSourceProps> = (props) => {
  const [, api] = useDataSources();
  const { state } = useLocation();
  const { id } = useParams();
  const { isShowing, toggle } = useModal();
  const [, { getDataSources }] = useLookup();

  const sourceId = Number(id);
  const [source, setSource] = React.useState<IDataSourceModel>(
    (state as any)?.source ?? defaultDataSource,
  );
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!!sourceId && source?.id !== sourceId) {
      api.getDataSource(sourceId).then((data) => {
        setSource({ ...data, parentId: data.parentId ? data.parentId : undefined });
      });
    }
  }, [api, source?.id, sourceId]);

  const handleSubmit = async (values: IDataSourceModel) => {
    try {
      const originalId = values.id;
      const model: IDataSourceModel = {
        ...values,
        topic: values.code,
        contentTypeId: values.contentTypeId,
        parentId: values.parentId ? values.parentId : undefined,
        ownerId: values.ownerId ? values.ownerId : undefined,
        connection: values.connection ? values.connection : {},
      };
      const result = !values.id
        ? await api.addDataSource(model)
        : await api.updateDataSource(model);
      setSource({ ...result, parentId: result.parentId ? result.parentId : 0 });
      getDataSources();
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/data/sources/${result.id}`);
    } catch {}
  };

  const hasErrors = (errors: any, props: string[]) => {
    return props.some((p) => !!errors[p]);
  };

  return (
    <styled.DataSourceForm>
      <Row>
        <IconButton
          iconType="back"
          label="Back to Sources"
          className="back-button"
          onClick={() => navigate('/admin/data/sources')}
        />
        <Col flex="1" className="info">
          <p>
            Sources provide a way to identify and manage different content sources and where they
            are ingested from. Each source represents one type of media. A service associated with
            the media type will ingest content which will become available when published.
          </p>
        </Col>
      </Row>
      <FormikForm
        initialValues={source}
        validationSchema={DataSourceSchema}
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
                      hasErrors={hasErrors(errors, ['name', 'code', 'mediaTypeId', 'licenseId'])}
                    />
                    <Tab navigateTo="metrics" label="Reach/Earned Media" />
                    <Tab
                      navigateTo="ingest/settings"
                      label="Ingest Settings"
                      hasErrors={hasErrors(errors, [
                        'dataLocationId',
                        'contentTypeId',
                        'topic',
                        'connection',
                      ])}
                    />
                    <Tab navigateTo="ingest/schedule" label="Ingest Schedule" />
                    <Tab navigateTo="ingesting" label="Ingesting" />
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
                  body="Are you sure you wish to remove this data source?"
                  isShowing={isShowing}
                  hide={toggle}
                  type="delete"
                  confirmText="Yes, Remove It"
                  onConfirm={async () => {
                    try {
                      await api.deleteDataSource(source);
                      toast.success(`${source.name} has successfully been deleted.`);
                      navigate('/admin/data/sources');
                    } finally {
                      toggle();
                    }
                  }}
                />
              </Tabs>
            </Col>
            <DataSourceStatus className="status" flex="1 1" />
          </Row>
        )}
      </FormikForm>
    </styled.DataSourceForm>
  );
};
