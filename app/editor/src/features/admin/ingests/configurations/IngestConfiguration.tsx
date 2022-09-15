import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { Show } from 'tno-core';

import { Audio, AVArchive, Image, Newspaper, Syndication, Video } from '.';
import * as styled from './styled';

// TODO: Hardcoded configuration values based on the media type name isn't good because it can be edited by an admin.
export const IngestConfiguration: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  return (
    <styled.Connection>
      <Show visible={!values.mediaTypeId}>Select a media type before configuring.</Show>
      <Show visible={values.mediaType?.name === 'Syndication'}>
        <Syndication />
      </Show>
      <Show visible={values.mediaType?.name === 'Paper'}>
        <Newspaper />
      </Show>
      <Show visible={values.mediaType?.name === 'AV Archive'}>
        <AVArchive />
      </Show>
      <Show visible={values.mediaType?.name === 'Audio'}>
        <Audio />
      </Show>
      <Show visible={values.mediaType?.name === 'Video'}>
        <Video />
      </Show>
      <Show visible={values.mediaType?.name === 'Image'}>
        <Image />
      </Show>
      <Show visible={values.mediaType?.name === 'HTML'}>
        <p>Under Construction</p>
      </Show>
      <Show visible={values.mediaType?.name === 'Social Media'}>
        <p>Under Construction</p>
      </Show>
    </styled.Connection>
  );
};
