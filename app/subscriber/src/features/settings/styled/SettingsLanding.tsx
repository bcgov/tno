import styled from 'styled-components';
import { Row } from 'tno-core';

export const SettingsLanding = styled(Row)<{ split?: boolean }>`
  width: 100%;
  .listIcon {
    color: ${(props) => props.theme.css.iconGrayColor};
  }
  .menuPanel {
    height: 100%;
    min-height: 85em;
  }
  .linkBox {
    cursor: pointer;
    div:hover {
      background-color: ${(props) => props.theme.css.iconGrayColor};
      color: ${(props) => props.theme.css.bkWhite};
    }
  }
  .left-side {
    @media (max-width: 1100px) {
      width: 100%;
    }
    @media (min-width: 1100px) {
      width: ${({ split }) => (split ? '35%' : '100%')};
    }
    hr {
      width: 95%;
    }
    p {
      margin-left: 1em;
      margin-top: 1.5em;
      font-weight: bold;
      color: ${(props) => props.theme.css.btnBkPrimary};
    }
    .description {
      margin-left: 1em;
    }
  }
  .right-side {
    @media (max-width: 1100px) {
      width: 100%;
    }
    @media (min-width: 1100px) {
      width: ${({ split }) => (split ? '65%' : '100%')};
    }
  }
  .header-row {
    width: 94%;
    .close-button {
      margin-left: auto;
      align-self: flex-end;
      margin-bottom: 0.75em;
      cursor: pointer;
      color: ${({ theme }) => theme.css.iconGrayColor};
      height: 1.2rem;
      width: 1.2rem;
    }
  }
`;
