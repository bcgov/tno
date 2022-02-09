import styled from 'styled-components';

import { INavBarGroupProps } from '.';

export const NavBarGroup = styled.div<INavBarGroupProps>`
  width: 100%;
  height: 40px;
  background-color: ${(props) => props.theme.css.primaryLightColor};
`;
