import { Button } from 'components/button';
import { Col, Row } from 'components/flex';
import { FormikForm } from 'components/formik';
import { IDataSourceModel, ScheduleType } from 'hooks/api-editor';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { useDataSources } from 'store/hooks/admin';

import { DataSourceDetails, ReachEarnedMedia, Schedule } from '.';
import { defaultSource } from './constants';
import * as styled from './styled';

interface IDataSourceProps {}

export const DataSource: React.FC<IDataSourceProps> = (props) => {
  const [api] = useDataSources();
  const { id } = useParams();
  const navigate = useNavigate();
  const { state } = useLocation();

  const [source, setSource] = React.useState<IDataSourceModel>(
    (state as any)?.source ?? defaultSource,
  );
  const sourceId = Number(id);

  React.useEffect(() => {
    if (source?.id !== sourceId) {
      api.getDataSource(sourceId).then((data) => {
        setSource(data);
        if (!!data.schedules.length) {
          // Navigate to the correct URL for the schedule type.
          if (data.schedules[0].scheduleType === ScheduleType.Repeating) {
            navigate('schedules/continuos', { replace: true, state: { source: data } });
          } else if (data.schedules.length === 1) {
            navigate('schedules/daily', { replace: true, state: { source: data } });
          } else {
            navigate('schedules/advanced', { replace: true, state: { source: data } });
          }
        }
      });
    }
  }, [api, navigate, source?.id, sourceId]);

  const handleSubmit = async (values: IDataSourceModel) => {
    const data = await api.updateDataSource(values);
    setSource(data);
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
          <Col>
            <Row alignItems="flex-start">
              <DataSourceDetails />
              <Schedule />
              <ReachEarnedMedia />
            </Row>
            <Col alignContent="flex-start" alignItems="flex-end">
              <Button type="submit">Save</Button>
            </Col>
          </Col>
        )}
      </FormikForm>
    </styled.DataSource>
  );
};
