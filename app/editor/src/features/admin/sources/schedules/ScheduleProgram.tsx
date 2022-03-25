import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { defaultSource } from '../constants';
import * as styled from './styled';

interface IScheduleProgramProps {
  values?: IDataSourceModel;
}

export const ScheduleProgram: React.FC<IScheduleProgramProps> = ({ values = defaultSource }) => {
  return <styled.Schedule className="schedule">Table</styled.Schedule>;
};
