import styled from 'styled-components';

import { ILogoPanelProps } from '..';

export const LogoPanel = styled.div<ILogoPanelProps>`
  background-color: ${(props) => props.backgroundColor};
  width: ${(props) => props.width};
  display: flex;
  border-radius: 20px 0px 0px 20px;
  justify-content: center;
  float: left;
  .logo {
    margin-top: 75%;
  }
`;

export default LogoPanel;
