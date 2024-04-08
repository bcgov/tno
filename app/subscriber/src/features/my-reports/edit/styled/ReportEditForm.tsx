import styled from 'styled-components';

export const ReportEditForm = styled.div`
  display: flex;
  flex-direction: column;

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

  .active-content {
    background-color: ${(props) => props.theme.css.highlightPrimary};
  }

  .info {
    > div {
      display: flex;
      flex-direction: row;
      gap: 1rem;
      align-items: center;
      background: ${(props) => props.theme.css.bkInfo};
      font-weight: 600;
    }
    color: ${(props) => props.theme.css.fInfo};
  }

  .section-options {
    padding-bottom: 1rem;
    border-bottom: solid 1px ${(props) => props.theme.css.lineTertiaryColor};

    .frm-in {
      padding: 0;
    }
  }

  .frm-in:not(.chk) {
    > label {
      text-transform: uppercase;
    }
  }
`;
