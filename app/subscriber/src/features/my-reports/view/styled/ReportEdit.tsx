import styled from 'styled-components';

export const ReportEdit = styled.div`
  .tab.report-name {
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0 1.5rem;
    background: ${(props) => props.theme.css.highlightPrimary};

    label {
      text-transform: uppercase;
    }
  }

  .sections {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    padding-top: 1rem;

    .content-row {
      display: flex;
      flex-direction: row;
      gap: 0.25rem;
      padding: 0.5rem 0;
      border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
    }
  }

  .preview-section {
    position: relative;
  }

  .preview-report {
    border: solid 2px ${(props) => props.theme.css.primaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.primaryLightColor};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
    }
  }
`;
