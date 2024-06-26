import styled from 'styled-components';

export const ReportEditSubscribersForm = styled.div`
  display: flex;
  flex-direction: column;
  margin: 1rem;
  gap: 1rem;
  .select-column {
    width: 5rem;
  }
  .selected-all-btn {
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
    width: 10rem;
  }
  .select-all-btn {
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
    margin-left: 0.25rem;
    width: 2rem;
  }
  .subscriber-block {
    display: flex;
    gap: 0.5rem;
    align-items: stretch;
    .subscriber-title {
      font-weight: bold;
      font-size: 1.25rem;
    }
    .subscriber-describe {
      font-size: 1rem;
      margin-top: 0.8rem;
      margin-bottom: 0.8rem;
    }
    .request-button {
      margin-left: 1em;
    }
  }
  .subscriber-exporter.empty {
    pointer-events: none;
    opacity: 0.5;
  }
  .hide {
    display: none;
  }
`;
