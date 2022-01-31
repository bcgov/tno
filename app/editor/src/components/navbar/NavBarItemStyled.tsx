import styled from 'styled-components';

import { INavBarItemProps } from '.';

export const NavBarItem = styled.div<INavBarItemProps>`
  width: 109px;
  height: 40px;
  background-color: ${(props) => (props.active ? '#65799e' : '#38598a')};
  color: white;
  font-weight: 500;
  padding-top: 0.5rem;
  text-align: center;
  &:hover {
    curosr: pointer;
  }
`;
