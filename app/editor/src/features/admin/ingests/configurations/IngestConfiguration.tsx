import { useFormikContext } from 'formik';
import React from 'react';
import { useConnections } from 'store/hooks/admin';
import { IIngestModel, Show } from 'tno-core';

import { Audio, AVArchive, DbMigration, FrontPage, Image, Newspaper, Syndication, Video } from '.';
import * as styled from './styled';

// TODO: Hardcoded configuration values based on the ingest type name isn't good because it can be edited by an admin.
export const IngestConfiguration: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  const [{ connections }, { findAllConnections }] = useConnections();

  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    // TODO: This doesn't handle failures after loading the first time.
    if (connections.length === 0 && loading) {
      setLoading(false);
      findAllConnections();
    }
  }, [loading, connections, findAllConnections]);

  return (
    <styled.Connection>
      <Show visible={!values.ingestTypeId}>Select a ingest type before configuring.</Show>
      <Show visible={values.ingestType?.name === 'Syndication'}>
        <Syndication />
      </Show>
      <Show visible={values.ingestType?.name === 'Paper'}>
        <Newspaper />
      </Show>
      <Show visible={values.ingestType?.name === 'AV Archive'}>
        <AVArchive />
      </Show>
      <Show visible={values.ingestType?.name === 'Audio'}>
        <Audio />
      </Show>
      <Show visible={values.ingestType?.name === 'Video'}>
        <Video />
      </Show>
      <Show visible={values.ingestType?.name === 'Image'}>
        <Image />
      </Show>
      <Show visible={values.ingestType?.name === 'Front Page'}>
        <FrontPage />
      </Show>
      <Show visible={values.ingestType?.name === 'HTML'}>
        <p>Under Construction</p>
      </Show>
      <Show visible={values.ingestType?.name === 'Social Media'}>
        <p>Under Construction</p>
      </Show>
      <Show visible={values.ingestType?.name === 'Corporate Calendar'}>
        <p>Under Construction</p>
      </Show>
      <Show visible={values.ingestType?.name === 'TNO-Image'}>
        <DbMigration />
      </Show>
      <Show visible={values.ingestType?.name === 'TNO-PrintContent'}>
        <DbMigration />
      </Show>
      <Show visible={values.ingestType?.name === 'TNO-AudioVideo'}>
        <DbMigration />
      </Show>
      <Show visible={values.ingestType?.name === 'TNO-Story'}>
        <DbMigration />
      </Show>
    </styled.Connection>
  );
};
