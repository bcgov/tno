import styled from 'styled-components';

export const ReportHistoryView = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;
  .report-history-headline {
    display: flex;
    align-items: first baseline;
    gap: 0.5em;
    

    > :nth-child(1) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
    > :nth-child(4) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    > :last-child {
      margin-left: auto;
      justify-content: flex-end;
            color: ${(props) => props.theme.css.iconPrimaryColor};

    }
  }


  .preview-report {
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
