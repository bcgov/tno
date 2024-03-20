import styled from 'styled-components';

export const ReportSendForm = styled.div`
  margin: 1rem;
  gap: 1rem;

  > div:last-child {
    button {
      margin-top: 1.5rem;
    }
  }

  .subscribers {
    padding: 1rem 0;
    gap: 0.25rem;

    .header {
      padding: 0.25rem;
      gap: 1rem;
      color: ${(props) => props.theme.css.fPrimaryColor};

      font-weight: 600;
      border-bottom: 2px solid ${(props) => props.theme.css.lineTertiaryColor};
    }

    .row {
      padding: 0.25rem;
      gap: 1rem;
      color: ${(props) => props.theme.css.fPrimaryColor};
      align-items: center;

      &:nth-child(even) {
        background: ${(props) => props.theme.css.highlightSecondary};
      }
    }
  }
`;
