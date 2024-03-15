import styled from 'styled-components';

export const ReportEditForm = styled.div`
  display: flex;
  flex-direction: column;

  .template-action-bar {
    justify-content: center;
    gap: 1rem;
    padding: 0.25rem;
    background: ${(props) => props.theme.css.highlightSecondary};
  }

  .report-template {
    gap: 0.5rem;

    > div {
      border-left: solid 1px ${(props) => props.theme.css.highlightSecondary};
      border-right: solid 1px ${(props) => props.theme.css.highlightSecondary};
    }

    > div:not(:first-child) {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
  }

  .preview-report {
    border: solid 2px ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin: 1rem;
    overflow: hidden;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
    }
  }

  .required::after {
    content: ' *';
    color: rgb(216, 41, 47);
  }
`;
