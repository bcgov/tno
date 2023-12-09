import styled from 'styled-components';
import { Row } from 'tno-core';

export const ContentActionBar = styled(Row)`
  align-items: center;
  padding: 1em 0.5em 0;
  font-size: 0.9em;
  width: 100%;
  svg {
    height: 1.25em;
    width: 1.25em;
  }
  .left-side-items {
    margin-left: 0.5em;
    color: ${(props) => props.theme.css.btnBkPrimary};
    &:hover {
      text-decoration: underline;
      cursor: pointer;
    }
  }

  .right-side-items {
    margin-left: auto;
    margin-right: 0.5em;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .action {
    display: flex;
    svg {
      align-self: flex-end;
      margin-right: 0.25em;
    }
    margin-right: 1em;
    &:hover {
      text-decoration: underline;
      cursor: pointer;
    }
  }
  &.list-view {
    background-color: ${(props) => props.theme.css.lineQuaternaryColor};
    margin-bottom: 1em;
    width: calc(100% + 2.4em);
    margin-left: -1.1em;
    margin-right: -1.2em;
    border: 1px solid ${(props) => props.theme.css.lineTertiaryColor};
    padding: 0.5em;
    .select-all {
      .check-area {
        display: flex;
        flex-direction: row;
        padding: 0.5em;
      }
      height: 2.5em;
      margin-top: -1.5em;
      margin-bottom: -1.5em;
      margin-left: -0.5em;
      background-color: ${(props) => props.theme.css.bkQuaternary};
    }
    .arrow {
      width: 0;
      height: 0;
      margin-top: -1.5em;
      margin-bottom: -1.5em;
      border-top: 1.3em solid transparent;
      border-bottom: 1.3em solid transparent;
      border-left: 1.5em solid ${(props) => props.theme.css.bkQuaternary};
    }
  }
`;
