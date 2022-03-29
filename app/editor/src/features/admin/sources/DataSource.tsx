import { Button } from 'components/button';
import { Col } from 'components/flex';
import { FormikForm } from 'components/formik';
import { Tab, Tabs } from 'components/tabs';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Outlet, useLocation, useParams } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin';

import { defaultSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSource: React.FC<IDataSourceProps> = (props) => {
  const [, api] = useDataSources();
  const { state } = useLocation();
  const { id } = useParams();

  const sourceId = Number(id);
  const [source, setSource] = React.useState<IDataSourceModel>(
    (state as any)?.source ?? defaultSource,
  );

  React.useEffect(() => {
    if (source?.id !== sourceId) {
      api.getDataSource(sourceId).then((data) => {
        setSource({ ...data, parentId: data.parentId ? data.parentId : undefined });
      });
    }
  }, [api, source?.id, sourceId]);

  const handleSubmit = async (values: IDataSourceModel) => {
    const data = await api.updateDataSource({
      ...values,
      parentId: values.parentId ? values.parentId : undefined,
    });
    setSource({ ...data, parentId: data.parentId ? data.parentId : undefined });
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
                {!!values.schedules.length && <Tab navigateTo="schedules" label="Schedule" />}
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
