import styled from 'styled-components';

import { ILogoPanelProps } from '..';

export const LogoPanel = styled.div<ILogoPanelProps>`
  background-color: ${(props) => props.backgroundColor};
  width: ${(props) => props.width};
  display: flex;
  border-radius: 20px 0px 0px 20px;
  @media only screen and (max-width: 1024px) {
    border-radius: 0px;
  }
  justify-content: center;
  float: left;
  @media only screen and (min-width: 1025px) {
    .logo {
      margin-top: 75%;
    }
  }
  flex: 1;
`;

export default LogoPanel;
