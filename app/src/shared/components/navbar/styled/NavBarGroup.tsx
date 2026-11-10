import styled from 'styled-components';

import { INavBarGroupProps } from '..';

export const NavBarGroup = styled.div<INavBarGroupProps>`
  display: flex;
  flex-direction: column;
  width: 100%;
  background-color: ${(props) => props.theme.css.primaryLightColor};
  z-index: ${(props) => props.hover && props.zIndex};
  position: ${(props) => props.hover && 'absolute'};

  & > div:not(:first-child) {
    border-top: solid 1px #65799e;

    & > div {
      border-right: solid 1px #65799e;
    }
  }
`;
