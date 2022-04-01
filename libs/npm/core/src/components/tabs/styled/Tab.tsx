import styled from 'styled-components';

import { ITabProps } from '..';

export const Tab = styled.div<ITabProps>`
  height: 40px;
  background-color: ${(props) => (props.active ? '#65799e' : '#38598a')};
  color: white;
  font-weight: 500;
  padding: 0.5em 1em 0 1em;
  text-align: center;
  cursor: pointer;
  border-top-left-radius: 0.35em;
  border-top-right-radius: 0.35em;
`;
