import styled from 'styled-components';

import { Col } from '../../flex';

export const ToolBarSection = styled(Col)`
  min-height: 7.25em;
  flex: 1 1 auto;

  @media screen and (max-width: 1847px) {
    padding: 0.2em;
  }
  @media screen and (min-width: 1847px) {
    padding: 0.5em;
  }

  .title-icon {
    color: ${(props) => props.theme.css.primaryColor};
    margin-left: auto;
    margin-right: auto;
    height: 1.5em;
    width: 1.5em;
  }

  .report-title {
    font-size: 1.25em;
    font-weight: 600;
  }

  .title-container {
    font-weight: 600;
    font-size: 1.25em;
  }

  .white-bg {
    background-color: ${(props) => props.theme.css.backgroundColor};
    padding: 0.25em;
    border-radius: 4px;
  }

  .title-children {
    border-radius: 4px;
  }

  .children-container {
    flex: 1 1 auto;
    padding: 0.5em;
    justify-content: center;
    background-color: ${(props) => props.theme.css.filterBackgroundColor};
    border-radius: 4px;
    min-height: 5.25em;
    align-items: center;

    .txt {
      background-color: ${(props) => props.theme.css.backgroundColor};
    }

    .small-txt {
      max-height: 1.5em;
    }

    .frm-in {
      padding-bottom: 0;
    }

    .chk {
      align-self: center;
    }

    .action-button {
      color: ${(props) => props.theme.css.actionButtonColor};
      height: 1.75em;
      width: 2em;
      cursor: pointer;
      margin-top: 0.25em;
      &:hover {
        color: ${(props) => props.theme.css.lightVariantColor};
      }
    }

    .icon-indicator {
      align-self: center;
      padding-left: 0.5em;
      padding-right: 0.25em;
      color: #bcbec5;
      height: 100%;
      width: 1.25em;
    }

    .select {
      margin-right: 0;
    }

    .action-icon {
      align-self: center;
      padding-left: 0.5em;
      padding-right: 0.25em;
      color: ${(props) => props.theme.css.primaryColor};
      height: 100%;
      width: 1.25em;
      cursor: pointer;
    }
  }

  .label-container {
    color: ${(props) => props.theme.css.lightLabelColor};
    font-size: 0.75em;
    padding: 0.5em;
    justify-content: center;

    svg {
      padding-right: 0.25em;
      align-self: center;
    }
  }

  .spaced {
    margin-left: 1em;
  }
`;
