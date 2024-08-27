import styled from 'styled-components';
import { Row } from 'tno-core';

export const SettingsLanding = styled(Row)<{ split?: boolean }>`
  width: 100%;
  .list-icon {
    color: ${(props) => props.theme.css.iconGrayColor};
    margin-right: 0.25em;
  }
  .menuPanel {
    height: 100%;
    min-height: 85em;
  }
  .link-box {
    cursor: pointer;
    div:hover {
      background-color: ${(props) => props.theme.css.iconGrayColor};
      color: ${(props) => props.theme.css.bkWhite};
    }
    margin-bottom: 1.5em;
    margin-left: 0.25em;
    margin-right: 0.25em;
  }
  .left-side {
    .page-section-title {
      margin-bottom: 0.25em;
    }
    @media (max-width: 1100px) {
      width: 100%;
    }
    @media (min-width: 1100px) {
      width: ${({ split }) => (split ? '35%' : '100%')};
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
  .link-box-container {
    padding: 0.75em;
    .description {
      font-weight: normal;
    }
    h2 {
      border-bottom: 1px solid ${(props) => props.theme.css.iconGrayColor};
      color: ${(props) => props.theme.css.linkPrimaryColor};
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
  @media (max-width: 768px) {
    .page-section {
      padding-bottom: 50px;
    }
  }
  @media (max-width: 853px) {
    .page-section {
      padding-bottom: 60px;
    }
  }
`;
