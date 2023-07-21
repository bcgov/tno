import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyMinisterSettings = styled(Col)`
  background-color: ${(props) => props.theme.css.lightGray};
  padding: 0.5em;
  .description {
  }

  .option-container {
    margin-top: 2em;
    background-color: ${(props) => props.theme.css.lightGray};
  }
  .chk-container {
    padding-bottom: 0.5rem;
    div {
      display: inline-block;
      vertical-align: text-top;
    }
    .aliases {
      padding-left: 1.75rem;
      display: block;
      font-style: italic;
      font-size: smaller;
    }
  }
  .ministers {
    & div:first-child {
      background-color: ${(props) => props.theme.css.lightGray};
      padding: 0.25em;
      span {
        margin-bottom: 0.25em;
      }
    }
  }
  .row-contents {
    justify-content: space-between;
  }

  button {
    background-color: ${(props) => props.theme.css.menuItemColor};
    &:hover {
      background-color: ${(props) => props.theme.css.selectedMenuItemColor};
    }
  }
`;
