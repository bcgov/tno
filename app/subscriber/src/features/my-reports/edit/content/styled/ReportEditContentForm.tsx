import styled from 'styled-components';

export const ReportEditContentForm = styled.div`
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 0.25rem;
  padding: 0.25rem 1rem;

  .section-actions {
    display: flex;
    flex-direction: row;
    justify-content: center;

    > div {
      flex: 1;
    }

    > div:nth-child(2) {
      display: flex;
      flex-direction: row;
      align-content: center;
      justify-content: center;
    }

    > div:last-child {
      display: flex;
      flex-direction: row;
      align-content: center;
      justify-content: flex-end;
      gap: 0.5rem;
    }
  }

  .sections {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
  }

  .excel-icon {
    height: 1.3em;
    padding: 0.15em;
    border-style: solid;
    border-width: 1px;
    border-color: #8084b1;
    border-radius: 0.2em;
  }
`;
