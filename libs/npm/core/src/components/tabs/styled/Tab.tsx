import styled from 'styled-components';

import { ITabProps } from '..';

export const Tab = styled.div<ITabProps>`
  background-color: ${(props) =>
    props.active ? '#fdd672' : props.hasErrors ? '#804d4d' : '#f2f2f2'};

  & .disabled {
    background-color: ${(props) =>
      props.active ? '#fdd672' : props.hasErrors ? '#804d4d' : '#F9F9F9'};
  }
`;
