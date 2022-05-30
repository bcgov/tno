import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks';
import React from 'react';

import { DataSourceMetrics } from '.';
import * as styled from './styled';

interface IReachEarnedMediaProps {}

export const ReachEarnedMedia: React.FC<IReachEarnedMediaProps> = () => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.ReachEarnedMedia className="reach-earned-media" alignItems="center">
      <h2>{values.name}</h2>
      <DataSourceMetrics />
    </styled.ReachEarnedMedia>
  );
};
