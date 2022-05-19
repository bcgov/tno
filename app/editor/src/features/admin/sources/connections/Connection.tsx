import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { Audio, AVArchive, Newspaper, Syndication, Video } from '.';
import * as styled from './styled';

export const Connection: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  let config;
  switch (values.mediaType?.name) {
    case 'Syndication':
      config = <Syndication />;
      break;
    case 'Newspaper':
    case 'Regional':
    case "Today's Edition":
      config = <Newspaper />;
      break;
    case 'AV Archive':
      config = <AVArchive />;
      break;
    case 'News Radio':
    case 'Talk Radio':
      config = <Audio />;
      break;
    case 'Television':
    case 'CC News':
    case 'CP News':
      config = <Video />;
      break;
  }

  return !!config ? <styled.Connection>{config}</styled.Connection> : null;
};
