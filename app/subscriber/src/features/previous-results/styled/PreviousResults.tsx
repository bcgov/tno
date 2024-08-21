import styled from 'styled-components';
import { Col } from 'tno-core';

export const PreviousResults = styled(Col)`
  .prev-result-row {
    &:hover {
      cursor: pointer;
      .hits {
        transform: scale(1.2);
      }
      .date {
        text-decoration: underline;
      }
    }
  }
  .hits {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0.25rem;
    margin-right: 1rem;
    margin-top: 0.25rem;
    color: white;
    font-weight: bold;
    height: 1rem;
    width: 1rem;
    border-radius: 50%;
    background-color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .date {
    height: 1rem;
    width: fit-content;
    align-items: center;
    padding: 0.25rem;

    color: ${(props) => props.theme.css.btnBkPrimary};
  }

  .loading {
    color: ${(props) => props.theme.css.btnBkPrimary};
    font-size: 1.5rem;
    margin-left: 0.25rem;
  }

  .loading:after {
    display: inline-block;
    animation: dotty steps(1, end) 1s infinite;
    content: '';
  }

  @keyframes dotty {
    0% {
      content: '';
    }
    25% {
      content: '.';
    }
    50% {
      content: '..';
    }
    75% {
      content: '...';
    }
    100% {
      content: '';
    }
  }
`;
