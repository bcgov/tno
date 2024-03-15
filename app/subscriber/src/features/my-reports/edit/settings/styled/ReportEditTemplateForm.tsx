import styled from 'styled-components';

export const ReportEditTemplateForm = styled.div`
  display: flex;
  flex-direction: column;

  .template-action-bar {
    justify-content: center;
    gap: 1rem;
    padding: 0.25rem;
    background: ${(props) => props.theme.css.highlightSecondary};
  }

  > div:not(:first-child) {
    display: flex;
    flex-direction: column;
    padding: 0 1rem;
  }

  > div:nth-child(2) {
    display: flex;
    flex-direction: row;
    gap: 0.25rem;
  }

  .report-template {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;

    > div:not(:first-child) {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
  }
`;
