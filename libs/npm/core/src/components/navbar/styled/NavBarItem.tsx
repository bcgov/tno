import styled from 'styled-components';

import { INavBarItemProps } from '..';

export const NavBarItem = styled.div<INavBarItemProps>`
  height: 40px;
  background-color: ${(props) => (props.active ? '#65799e' : '#38598a')};
  color: white;
  font-weight: 500;
  padding-top: 0.5rem;
  padding: 0.5em 1em 0 1em;
  text-align: center;
  cursor: pointer;

  &:hover {
    background-color: #65799e;
  }
`;
