import styled from 'styled-components';

import { IMenuToggleProps } from '..';

export const MenuToggle = styled.div<IMenuToggleProps>`
  button {
    margin: 0px;
    padding: 0px;
    width: ${(props) => props.width ?? '50px'};
    height: ${(props) => props.height ?? '50px'};
    border: solid 1px #ffffff;
  }

  svg {
    width: calc(100% - 13px);
    margin: 0px;
    padding: 0px;
  }
`;

export default MenuToggle;
