import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyMinisterSettings = styled(Col)`
  background-color: ${(props) => props.theme.css.lightGray};
  padding: 0.5em;
  .description {
    margin-bottom: 2em;
  }

  .options {
    margin-bottom: 10px;
  }

  .option-container {
    background-color: ${(props) => props.theme.css.lightGray};
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
