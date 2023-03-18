import { useFormikContext } from 'formik';
import React from 'react';
import { ISourceModel } from 'tno-core';

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
