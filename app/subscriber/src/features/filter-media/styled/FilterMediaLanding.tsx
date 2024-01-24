import styled from 'styled-components';
import { Row } from 'tno-core';

export const FilterMediaLanding = styled(Row)`
  .filters {
    font-size: 1.2rem;
    width: 40%;
    .show-all {
      font-weight: bold;
      cursor: pointer;
      &:hover {
        text-decoration: underline;
      }
    }
    .media-filter {
      margin-left: 1rem;
      margin-bottom: 0.5rem;
      border-right: 1px solid black;
      width: fit-content;
      padding-right: 1rem;
      .active {
        position: relative;
        color: ${(props) => props.theme.css.bkWhite};
        background-color: ${(props) => props.theme.css.btnBkPrimary};
      }
      .active::after {
        content: '';
        position: absolute;
        top: 0;
        right: -0.75rem;
        border-top: 1rem solid transparent;
        border-bottom: 1rem solid transparent;
        border-left: 0.75rem solid ${(props) => props.theme.css.btnBkPrimary};
      }
      .option {
        cursor: pointer;
        padding-left: 1rem;
        display: flex;
        align-items: center;
        height: 2rem;
        align-items: center;
        width: 10rem;
      }
    }
    .narrowed-options {
      margin-left: 1rem;
      .active-narrowed-option {
        color: ${(props) => props.theme.css.bkWhite};
        background-color: ${(props) => props.theme.css.btnBkPrimary};
        cursor: pointer;
      }
      .inactive-narrowed-option {
        cursor: pointer;
      }
      .alpha-filter {
        .active-letter {
          color: ${(props) => props.theme.css.bkWhite};
          background-color: ${(props) => props.theme.css.btnBkPrimary};
        }
        font-weight: bold;
        div {
          padding-left: 0.25rem;
          padding-right: 0.25rem;
          cursor: pointer;
        }
      }
    }
  }
  .results {
    width: 60%;
  }
`;
