import styled from 'styled-components';
import { Col } from 'tno-core';

export const ToolBarSection = styled(Col)`
  padding: 0.5em;
  min-height: 7.25em;
  .title-container {
    font-weight: 600;
    font-size: 1.25em;
    margin-top: 1em;
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

    .txt {
      background-color: ${(props) => props.theme.css.backgroundColor};
    }
    .small-txt {
      max-height: 1.5em;
    }
    .frm-in {
      padding-bottom: 0;
    }
    padding: 0.5em;
    justify-content: center;
    .chk {
      align-self: center;
    }
    .action-button {
      color: ${(props) => props.theme.css.actionButtonColor};
      :hover {
        color: ${(props) => props.theme.css.lightVariantColor};
      }
      height: 1.75em;
      width: 2em;
      cursor: pointer;
      margin-top: 0.25em;
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
      color: ${(props) => props.theme.css.primaryColor}
      height: 100%;
      width: 1.25em;
      cursor: pointer;
    }
    background-color: ${(props) => props.theme.css.filterBackgroundColor};
    border-radius: 4px;
    min-height: 5.25em;
    align-items: center;
  }
  .label-container {
    svg {
      padding-right: 0.25em;
    }
    color: ${(props) => props.theme.css.lightLabelColor};
    font-size: 0.75em;
    padding: 0.5em;
    justify-content: center;
  }
`;
