import styled from 'styled-components';

export const ReportView = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;

  .report-edit-headline-row {
    display: flex;
    align-items: first baseline;
    gap: 0.5em;

    > :nth-child(1) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    > :last-child {
      margin-left: auto;
      justify-content: flex-end;
    }
  }

  .preview-report {
    position: relative;

    .spinner {
      position: fixed;
    }

    .preview-subject {
      padding: 1rem;
      background-color: ${(props) => props.theme.css.btnBkPrimary};
      color: #fff;
    }

    .preview-body {
      padding: 1rem;
      img {
        max-width: 100%;
      }
    }
  }
`;
