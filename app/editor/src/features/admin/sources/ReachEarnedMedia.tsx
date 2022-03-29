import React from 'react';

import { DataSourceMetrics } from '.';
import * as styled from './styled';

interface IReachEarnedMediaProps {}

export const ReachEarnedMedia: React.FC<IReachEarnedMediaProps> = () => {
  return (
    <styled.ReachEarnedMedia className="reach-earned-media" alignItems="center">
      <DataSourceMetrics />
    </styled.ReachEarnedMedia>
  );
};
