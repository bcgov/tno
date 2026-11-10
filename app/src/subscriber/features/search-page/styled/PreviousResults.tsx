import styled from 'styled-components';
import { Col } from 'tno-core';

export const PreviousResults = styled(Col)`
  .prev-result-row {
    padding-left: 1rem;
    padding-bottom: 0.5rem;
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
  .no-results {
    margin-left: 1rem;
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
`;
