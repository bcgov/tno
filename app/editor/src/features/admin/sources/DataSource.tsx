import { FormikForm } from 'components/formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Outlet, useLocation, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useDataSources } from 'store/hooks/admin';
import { Button } from 'tno-core';
import { Col } from 'tno-core/dist/components/flex';
import { Tab, Tabs } from 'tno-core/dist/components/tabs';

import { defaultDataSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSource: React.FC<IDataSourceProps> = (props) => {
  const [, api] = useDataSources();
  const { state } = useLocation();
  const { id } = useParams();

  const sourceId = Number(id);
  const [source, setSource] = React.useState<IDataSourceModel>(
    (state as any)?.source ?? defaultDataSource,
  );

  React.useEffect(() => {
    if (source?.id !== sourceId) {
      api.getDataSource(sourceId).then((data) => {
        setSource({ ...data, parentId: data.parentId ? data.parentId : undefined });
      });
    }
  }, [api, source?.id, sourceId]);

  const handleSubmit = async (values: IDataSourceModel) => {
    try {
      const data = await api.updateDataSource({
        ...values,
        parentId: values.parentId ? values.parentId : undefined,
      });
      setSource({ ...data, parentId: data.parentId ? data.parentId : 0 });
      toast.success(`${data.name} has successfully been saved.`);
    } catch {}
  };

  return (
    <styled.DataSource>
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
        {({ values, errors, touched, handleChange, handleBlur, handleSubmit, isSubmitting }) => (
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
            <Col alignContent="flex-start" alignItems="flex-end">
              <Button type="submit">Save</Button>
            </Col>
          </Tabs>
        )}
      </FormikForm>
    </styled.DataSource>
  );
};
