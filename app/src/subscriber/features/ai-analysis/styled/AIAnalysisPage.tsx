import styled from 'styled-components';
import { Col } from 'tno-core';

export const AIAnalysisPage = styled(Col)`
  .two-col {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    padding: 1rem;
    flex: 1;
    align-items: flex-start;
  }

  .prompt-col {
    display: flex;
    flex-direction: column;
    flex: 0 0 33.333%;
    gap: 0.5rem;
  }

  .response-col {
    display: flex;
    flex-direction: column;
    flex: 1;
    gap: 0.5rem;
  }

  .col-label {
    font-size: 0.875rem;
    font-weight: 600;
    color: ${({ theme }) => theme.css.hPrimaryColor};
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }

  .prompt-actions {
    display: flex;
    justify-content: flex-end;
    margin-top: 0.25rem;
  }

  .response-panel {
    border: 1px solid ${({ theme }) => theme.css.linePrimaryColor};
    border-radius: 4px;
    padding: 1rem;
    min-height: 300px;
    background: ${({ theme }) => theme.css.bkWhite};
    color: ${({ theme }) => theme.css.fPrimaryColor};
    line-height: 1.6;

    &.loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      color: ${({ theme }) => theme.css.iconGrayColor};
    }

    &.empty {
      color: ${({ theme }) => theme.css.iconGrayColor};
      font-style: italic;
    }

    p {
      margin: 0 0 0.5rem;
    }
  }
`;
