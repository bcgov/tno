import styled from 'styled-components';

export const ReportViewForm = styled.div`
  display: flex;
  flex-direction: column;
  margin: 1rem;
  gap: 1rem;

  .send-test-button {
    margin-left: 1rem;
  }
  .preview-block-headline {
    font-weight: bold;
    font-size: 1.25rem;
  }
  .preview-send-details-row {
    margin-left: 1.5rem;
    gap: 1rem;
  }
  .hide {
    display: none;
  }
`;
