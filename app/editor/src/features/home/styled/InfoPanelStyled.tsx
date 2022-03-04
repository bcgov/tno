import styled from 'styled-components';

import { IInfoPanelProps } from '..';

export const InfoPanel = styled.div<IInfoPanelProps>`
  background-color: ${(props) => props.backgroundColor};
  width: ${(props) => props.width};
  display: flex;
  flex-direction: column;
  justify-content: center;
  float: left;
  border-radius: ${(props) => props.roundedEdges && '0px 20px 20px 0px'};
  .info {
    padding: 65px 60px 65px 60px;
    font-size: 16px;
    line-height: 26px;
    .email {
      margin-top: 25px;
    }
  }

  .loginPanel {
    margin-left: 60px;
    .copyright {
      margin-top: 200px;
      margin-bottom: 25px;
      font-size: 13px;
      line-height: 16px;
      width: 280px;
      height: 113px;
    }
    .headerSection {
      margin-top: 100px;
      .signIn {
        width: 96px;
        height: 40px;
      }
    }
  }
`;

export default InfoPanel;
