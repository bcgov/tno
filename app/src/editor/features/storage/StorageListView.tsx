import React from 'react';
import { useParams } from 'react-router';

import { useQuery } from '@/shared';

import { FileManager } from '.';

const StorageListView: React.FC = (props) => {
  const { id } = useParams();
  const query = useQuery();

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

export default StorageListView;
