import { IOptionItem, OptionItem } from 'components/form';
import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { DataSourceScheduleTypeName, IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { ActionMeta } from 'react-select';
import ReactTooltip from 'react-tooltip';
import { useLookup } from 'store/hooks';
import { Col, Row } from 'tno-core';
import { getSortableOptions } from 'utils';

import { Connection } from '../connections';
import * as styled from './styled';

interface IServiceIngestSettingsProps {}

export const ServiceIngestSettings: React.FC<IServiceIngestSettingsProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();

  const showKafkaTopic = values.connection.serviceType !== 'stream';
  const users = [
    new OptionItem('None', 0),
    ...lookups.users.map((u) => new OptionItem(u.username, u.id)),
  ];
  const contentTypes = getSortableOptions(lookups.contentTypes);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const owner = lookups.users.find((mt) => mt.id === values.ownerId);
    setFieldValue('owner', owner);
  }, [lookups.users, setFieldValue, values.ownerId, values.owner]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const handleOwnerChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const owner = lookups.users.find((mt) => mt.id === option.value);
    setFieldValue('owner', owner);
  };

  const handleContentTypeChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const contentType = lookups.contentTypes.find((mt) => mt.id === option.value);
    setFieldValue('contentType', contentType);
  };

  return (
    <styled.ServiceIngestSettings className="schedule">
      <h2>{values.name}</h2>
      <p>
        Data source connection settings provide the configuration that enables the service to ingest
        content.
      </p>
      <Row colGap="1em" nowrap>
        <Col flex="1 1">
          <FormikSelect
            label="Content Type"
            name="contentTypeId"
            tooltip="The type of content that is created when ingesting"
            options={contentTypes}
            required
            onChange={handleContentTypeChange}
          />
          <FormikSelect
            label="Owner"
            name="ownerId"
            tooltip="The default user who will own ingested content"
            options={users}
            onChange={handleOwnerChange}
          />
          {showKafkaTopic && (
            <FormikText
              label="Kafka Topic"
              name="topic"
              required={values.scheduleType !== DataSourceScheduleTypeName.None}
            />
          )}
          {showKafkaTopic && (
            <div>
              <p>
                A Kafka Topic is a category/feed name to which records are stored and published. If
                this data-source has a running service, the content will be ingested and placed in
                the Kafka Event Streaming data storage location.
              </p>
              <p>
                The topic should be unique, and all content stored within it should be the same
                format.
              </p>
            </div>
          )}
        </Col>
        <Col flex="1 1">
          <Connection />
        </Col>
      </Row>
    </styled.ServiceIngestSettings>
  );
};
