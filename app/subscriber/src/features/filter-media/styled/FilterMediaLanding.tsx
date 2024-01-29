import styled from 'styled-components';
import { Row } from 'tno-core';

export const FilterMediaLanding = styled(Row)`
  .filters {
    .scroll-container {
      max-height: calc(100dvh - 20rem);
      overflow-y: auto;
    }
    .page-section {
      position: fixed;
      top: 4.25rem;
      width: 40%;
    }
    font-size: 1.2rem;
    width: 45%;
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
        right: -0.7rem;
        border-top: 1rem solid transparent;
        border-bottom: 1rem solid transparent;
        border-left: 0.75rem solid ${(props) => props.theme.css.btnBkPrimary};
      }
      .option {
        cursor: pointer;
        &:hover {
          text-decoration: underline;
        }
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
      margin-bottom: 0.5rem;
      overflow-y: auto;
      .narrowed-option {
        padding: 0 0.25rem 0 0.25rem;
        cursor: pointer;
        &:hover {
          text-decoration: underline;
        }
        max-width: fit-content;
      }
      .active {
        color: ${(props) => props.theme.css.bkWhite};
        background-color: ${(props) => props.theme.css.btnBkPrimary};
      }
      .alpha-filter {
        .active-letter {
          color: ${(props) => props.theme.css.bkWhite};
          background-color: ${(props) => props.theme.css.btnBkPrimary};
        }
        font-weight: bold;
        div {
          &:hover {
            text-decoration: underline;
            transform: scale(1.3);
          }
          padding-left: 0.25rem;
          padding-right: 0.25rem;
          cursor: pointer;
        }
      }
    }
  }
  .results {
    width: 55%;
    .table {
      max-height: calc(100dvh - 20rem);
      overflow-y: auto;
    }
  }
`;
