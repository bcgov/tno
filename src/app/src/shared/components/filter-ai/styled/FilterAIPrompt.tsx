import styled from 'styled-components';

export const FilterAIPrompt = styled.div`
  display: flex;
  flex-direction: column;
  gap: 0.5rem;

  .filter-ai-actions {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.5rem;

    .filter-ai-button svg {
      margin-right: 0.35rem;
    }
  }

  .filter-ai-status {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.9rem;
  }

  .filter-ai-error {
    color: #b00020;
    font-size: 0.9rem;
  }

  .filter-ai-explanation {
    font-size: 0.9rem;
    color: #444;
    white-space: pre-wrap;
  }
`;
