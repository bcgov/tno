import { useFormikContext } from 'formik';
import { ISourceModel } from 'hooks';
import React from 'react';

import { SourceMetrics } from '.';
import * as styled from './styled';

interface IReachEarnedMediaProps {}

export const ReachEarnedMedia: React.FC<IReachEarnedMediaProps> = () => {
  const { values } = useFormikContext<ISourceModel>();

  return (
    <styled.ReachEarnedMedia className="reach-earned-media" alignItems="center">
      <h2>{values.name}</h2>
      <SourceMetrics />
    </styled.ReachEarnedMedia>
  );
};
