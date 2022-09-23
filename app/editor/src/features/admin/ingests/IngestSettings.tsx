import { FormikCheckbox, FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { useConnections } from 'store/hooks/admin';
import { Col, Row, Section, Show } from 'tno-core';
import { getSortableOptions } from 'utils';

import { IngestConfiguration } from './configurations';
import * as styled from './styled';

interface IIngestSettingsProps {}

/**
 * A UI component form to manage data source ingest settings.
 * @returns Component.
 */
export const IngestSettings: React.FC<IIngestSettingsProps> = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const [{ connections }, { findAllConnections }] = useConnections();
  const [lookups] = useLookup();
  useTooltips();

  const [loading, setLoading] = React.useState(true);

  const ingestTypes = getSortableOptions(lookups.ingestTypes);
  const connectionOptions = getSortableOptions(connections);

  React.useEffect(() => {
    if (loading && connections.length === 0) {
      setLoading(false);
      findAllConnections();
    }
  }, [connections.length, findAllConnections, loading]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const ingestType = lookups.ingestTypes.find((mt) => mt.id === values.ingestTypeId);
    setFieldValue('ingestType', ingestType);
  }, [lookups.ingestTypes, setFieldValue, values.ingestTypeId, values.ingestType]);

  return (
    <styled.IngestSettings className="schedule">
      <h2>{values.name}</h2>
      <p>
        Data source configuration settings provide the configuration that enables the service to
        ingest content. Each ingest type may have different configuration options.
      </p>
      <Show visible={values.ingestType?.name === 'Audio'}>
        <p>
          Audio can be captured as both a stream and clip. Audio clips can be extracted from an
          active stream. This enables running the clip service separately to generate clips.
        </p>
      </Show>
      <Show visible={values.ingestType?.name === 'Video'}>
        <p>
          Video can only create clips from a completed file. Video streams must be complete before
          they are used for clips. This requires the stream service to be used to generate clips.
        </p>
      </Show>
      <Row colGap="1em" nowrap>
        <Col flex="1 1">
          <h3>Connection</h3>
          <Section>
            <p>Select the appropriate ingest type that this service will ingest.</p>
            <FormikSelect label="Ingest Type" name="ingestTypeId" options={ingestTypes} required />
            <p>
              If the ingest service will connect to a remote source of data select the connection,
              otherwise select 'None'.
            </p>
            <FormikSelect
              label="Source"
              name="sourceConnectionId"
              options={connectionOptions}
              onChange={(newValue: any) => {
                const source = connections.find((c) => c.id === newValue.value);
                if (!!source) setFieldValue('sourceConnection', source);
              }}
              required
            />
            <p>
              If the ingest service will generate files select the connection that represents the
              destination for these files, otherwise select 'None'.
            </p>
            <FormikSelect
              label="Destination"
              name="destinationConnectionId"
              options={connectionOptions}
              onChange={(newValue: any) => {
                const source = connections.find((c) => c.id === newValue.value);
                if (!!source) setFieldValue('destinationConnection', source);
              }}
              required
            />
            <p>
              Select if content should be posted to Kafka. If the service doesn't generate content
              it doesn't need to capture.
            </p>
            <FormikCheckbox label="Capture Content" name="configuration.post" />
            <p>
              Select if content will be imported by the content service. This provides a way to
              continue ingesting content to Kafka without indexing it.
            </p>
            <FormikCheckbox label="Import Content" name="configuration.import" />
          </Section>
        </Col>
        <Col flex="1 1">
          <h3>Configuration</h3>
          <IngestConfiguration />
        </Col>
      </Row>
    </styled.IngestSettings>
  );
};
