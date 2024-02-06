import styled from 'styled-components';
import { Row } from 'tno-core';

export const ContentActionBar = styled(Row)`
  &.search {
    .check-area {
      margin-left: 1em;
    }
  }
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

  @keyframes fade-in {
    from {
      opacity: 0;
      visibility: hidden;
    }
    to {
      opacity: 1;
      visibility: visible;
    }
  }

  .right-side-items {
    margin-left: auto;
    color: ${(props) => props.theme.css.btnBkPrimary};
    @media (max-width: 768px) {
      span {
        display: none;
      }
    }
    animation: fade-in 0.5s linear;
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
    border: 1px solid ${(props) => props.theme.css.lineTertiaryColor};
    padding: 0em;
    .select-all {
      .check-area {
        @media (max-width: 768px) {
          margin-left: 0.5em;
        }
        display: flex;
        flex-direction: row;
        padding: 0.5em 0.5em 0.5em 0em;
      }
      height: 2.5em;
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
  .back-button {
    height: 1.3em;
    padding: 0.15em;
  }
  .back-tooltip {
    height: 1em;
    width: 2em;
    padding-top: 0.25em;
    background-color: ${(props) => props.theme.css.bkWhite};
    color: black;
    border: 1.5px solid ${(props) => props.theme.css.navItemSecondaryBackgroundColor};
    font-size: 1em;
    border-radius: 0.5em;
  }
  .back-tooltip-arrow {
    border: 1.5px solid ${(props) => props.theme.css.navItemSecondaryBackgroundColor};
    border-top: 0px;
    border-left: 0px;
    margin-bottom: -1.5px;
  }
`;
