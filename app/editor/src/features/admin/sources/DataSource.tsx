import { IconButton } from 'components/form';
import { FormikForm } from 'components/formik';
import { Modal } from 'components/modal';
import { useModal } from 'hooks';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Outlet, useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { useDataSources } from 'store/hooks/admin';
import { Button, ButtonVariant } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';
import { Tab, Tabs } from 'tno-core/dist/components/tabs';

import { defaultDataSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSource: React.FC<IDataSourceProps> = (props) => {
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
        parentId: values.parentId ? values.parentId : undefined,
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

  return (
    <styled.DataSource>
      <IconButton
        iconType="back"
        label="Back to Sources"
        className="back-button"
        onClick={() => navigate('/admin/data/sources')}
      />
      <FormikForm
        initialValues={source}
        validate={(values) => {
          const errors = {};
          return errors;
        }}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting }) => (
          <Tabs
            tabs={
              <>
                <Tab navigateTo="details" label="Details" exact activePaths={[`${id}`]} />
                <Tab navigateTo="schedule" label="Schedule" />
                <Tab navigateTo="metrics" label="Reach/Earned Media" />
              </>
            }
          >
            <Outlet />
            <Row justify="flex-end" className="form-actions">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                Delete
              </Button>
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
        )}
      </FormikForm>
    </styled.DataSource>
  );
};
