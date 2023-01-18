import styled from 'styled-components';

import { ITabProps } from '..';

export const Tab = styled.div<ITabProps>`
  background-color: ${(props) =>
    props.active ? '#fdd672' : props.hasErrors ? '#804d4d' : '#f2f2f2'};
  color: #494949;
  font-weight: bold;
  padding: 0.5em 1em 0.5em 1em;
  text-align: center;
  cursor: pointer;
  border-top-left-radius: 0.35em;
  border-top-right-radius: 0.35em;
`;
