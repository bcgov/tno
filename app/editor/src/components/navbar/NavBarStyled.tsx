import styled from 'styled-components';

import { INaVBarProps } from '.';

export const NavBar = styled.div<INaVBarProps>`
  width: 100%;
  height: 40px;
  background-color: #38598a;
  .item:hover {
    cursor: pointer;
  }
`;
