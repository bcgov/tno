import { Button } from 'components/button';
import { Row } from 'components/flex';
import { FormikForm } from 'components/formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useParams } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin';

import { DataSourceDetails, ReachEarnedMedia, Schedule } from '.';
import { defaultSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSource: React.FC<IDataSourceProps> = (props) => {
  const [api] = useDataSources();
  const { id } = useParams();

  const [source, setSource] = React.useState<IDataSourceModel>();
  const sourceId = Number(id);

  React.useEffect(() => {
    api.getDataSource(sourceId).then((data) => {
      setSource(data);
    });
  }, [api, sourceId]);

  const handleSubmit = async (values: IDataSourceModel) => {
    const data = await api.updateDataSource(values);
    setSource(data);
  };

  return (
    <styled.DataSource>
      <FormikForm
        initialValues={source ?? defaultSource}
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
          <Row alignItems="flex-start">
            <DataSourceDetails values={source} />
            <Schedule values={source} />
            <ReachEarnedMedia values={source} />
            <Button type="submit">Save</Button>
          </Row>
        )}
      </FormikForm>
    </styled.DataSource>
  );
};
