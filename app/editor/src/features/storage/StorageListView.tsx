import { useQuery, useTooltips } from 'hooks';
import React from 'react';
import { useParams } from 'react-router-dom';

import { FileManager } from '.';

export const StorageListView: React.FC = (props) => {
  const { id } = useParams();
  const query = useQuery();
  useTooltips();

  const [, setClipErrors] = React.useState<string>('');

  return (
    <FileManager
      setClipErrors={setClipErrors}
      locationId={parseInt(id ?? '1')}
      showLocations={true}
      path={query.get('path') ?? ''}
    />
  );
};
