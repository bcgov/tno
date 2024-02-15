import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyMinisterSettings = styled(Col)`
  padding: 0.5em;
  .description {
  }

  .option-container {
    margin-top: 2em;
  }
  .chk-container {
    padding-bottom: 0.5rem;
    div {
      display: inline-block;
      vertical-align: text-top;
    }
    div.chk {
      div {
        label {
          font-weight: bold;
        }
      }
    }
  }
  .ministers {
    & div:first-child {
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
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    border-radius: 0.25em;
  }
`;
