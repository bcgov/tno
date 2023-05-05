import styled from 'styled-components';

import { IInfoPanelProps } from '..';

export const InfoPanel = styled.div<IInfoPanelProps>`
  background-color: ${(props) => props.backgroundColor};
  width: ${(props) => props.width};
  display: flex;
  flex-direction: column;
  justify-content: center;
  float: left;
  flex: 1;
  border-radius: ${(props) => props.roundedEdges && '0px 20px 20px 0px'};

  .info {
    padding: 65px 60px 65px 60px;
    font-size: 16px;
    line-height: 26px;
    .email {
      margin-top: 25px;
    }
    .alert-message {
      max-height: 13em;
      overflow: scroll;
      overflow-x: hidden;
    }
  }

  .loginPanel {
    margin-left: 40px;
    @media only screen and (max-width: 1024px) {
      margin-left: 30px;
      margin-right: 30px;
    }
    .copyright {
      margin-top: 100px;
      margin-bottom: 25px;
      font-size: 13px;
      line-height: 16px;
      @media only screen and (min-width: 281px) {
        width: 280px;
        height: 113px;
      }
    }
    .headerSection {
      margin-top: 100px;
      .signIn {
        text-align: center;
      }
    }

    button {
      width: 80px;

      & > div {
        justify-content: center;
      }
    }
  }
`;
