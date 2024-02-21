import styled from 'styled-components';

export const ReportView = styled.div`
  .page-section {
    position: relative;
    min-height: 200px;
  }

  .preview-report {
    border: solid 2px ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 0.5rem;
    display: flex;
    flex-flow: column;
    box-shadow: 0 3px 15px rgb(0 0 0 / 0.5);
    margin-top: 1rem;

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
    }
  }
`;
