import { FieldSize, IOptionItem, OptionItem } from 'components/form';
import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { DataSourceScheduleTypeName, IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { ActionMeta } from 'react-select';
import ReactTooltip from 'react-tooltip';
import { useLookup } from 'store/hooks';
import { Col, Row } from 'tno-core';

import { Connection } from '../media-types';
import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import { scheduleTypeOptions } from './constants';
import * as styled from './styled';

interface IServiceConfigProps {}

export const ServiceConfig: React.FC<IServiceConfigProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();

  const users = [
    new OptionItem('None', 0),
    ...lookups.users.map((u) => new OptionItem(u.username, u.id)),
  ];

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const owner = lookups.users.find((mt) => mt.id === values.ownerId);
    setFieldValue('owner', owner);
  }, [lookups.users, setFieldValue, values.ownerId, values.owner]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const form = (scheduleType: DataSourceScheduleTypeName) => {
    switch (scheduleType) {
      case DataSourceScheduleTypeName.Continuous:
        return <ScheduleContinuous index={0} />;
      case DataSourceScheduleTypeName.Daily:
        return <ScheduleDaily index={0} />;
      case DataSourceScheduleTypeName.Advanced:
        return <ScheduleAdvanced />;
    }
  };

  const handleOwnerChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const owner = lookups.users.find((mt) => mt.id === option.value);
    setFieldValue('owner', owner);
  };

  return (
    <styled.ServiceConfig className="schedule">
      <p>
        A service schedule provides a way to manage when and how often source content is imported.
      </p>
      <Row colGap="1em" nowrap>
        <Col>
          <FormikSelect
            label="Schedule Type"
            name="scheduleType"
            options={scheduleTypeOptions}
            width={FieldSize.Medium}
            onChange={(newValue: any) => {
              if (
                newValue.value === DataSourceScheduleTypeName.None &&
                values.schedules.some((s) => s.id === 0)
              ) {
                setFieldValue(
                  'schedules',
                  values.schedules.filter((s) => s.id !== 0),
                );
              } else if (!values.topic) {
                // Default value to unique code, but allow override.
                setFieldValue('topic', values.code);
              }
            }}
          />
          <FormikSelect
            label="Owner"
            name="ownerId"
            tooltip="The default user who will own ingested content"
            options={users}
            onChange={handleOwnerChange}
          />
          <FormikText
            label="Kafka Topic"
            name="topic"
            width={FieldSize.Medium}
            disabled={values.scheduleType === DataSourceScheduleTypeName.None}
            required={values.scheduleType !== DataSourceScheduleTypeName.None}
          />
        </Col>
        <Col flex="1 1 50%">
          <p>
            A Kafka Topic is a category/feed name to which records are stored and published. If this
            data-source has a running service, the content will be ingested and placed in the Kafka
            Event Streaming data storage location.
          </p>
          <p>
            The topic should be unique, or all content stored within it should be the same format.
          </p>
        </Col>
        <Col>
          <Connection />
        </Col>
      </Row>
      <hr />
      {form(values.scheduleType)}
    </styled.ServiceConfig>
  );
};
