import styled from 'styled-components';
import { Col } from 'tno-core';

export const ToolBarSection = styled(Col)`
  padding: 0.5em;

  .children-container {
    padding: 0.5em;
    justify-content: center;
    .action-button {
      color: #007af5;
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
      color: #003366;
      height: 100%;
      width: 1.25em;
      cursor: pointer;
    }
    background-color: #f2f2f2;
    border-radius: 4px;
    min-height: 4.35em;
    align-items: center;
  }
  .label-container {
    svg {
      padding-right: 0.25em;
    }
    color: #9295a0;
    font-size: 0.75em;
    padding: 0.5em;
    justify-content: center;
  }
`;
