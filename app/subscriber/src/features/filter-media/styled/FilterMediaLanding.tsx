import styled from 'styled-components';
import { Row } from 'tno-core';

export const FilterMediaLanding = styled(Row)`
  .filters {
    .scroll-container {
      max-height: calc(100dvh - 20rem);
      overflow-y: auto;
    }
    font-size: 1.2rem;
    @media (min-width: 1000px) {
      width: 30%;
    }
    @media (max-width: 1000px) {
      width: 100%;
    }
    .show-all {
      font-weight: bold;
      padding: 0.25em;
      cursor: pointer;
      &:hover {
        text-decoration: underline;
      }
    }
    .main-media {
      flex-flow: nowrap;
    }
    .all-chk {
      cursor: pointer;
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
    .opt-chk {
      margin-left: auto;
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
        max-width: 100%;
      }
      .alpha-filter {
        flex-flow: wrap;
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
    .results-side {
      overflow-y: auto;
      padding: 0.5rem;
    }
    @media (min-width: 1000px) {
      width: 70%;
    }
    @media (max-width: 1000px) {
      width: 100%;
    }
  }
`;
