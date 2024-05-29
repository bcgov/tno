import styled from 'styled-components';

export const ReportSendForm = styled.div`
  display: flex;
  flex-direction: column;
  margin: 1rem;
  gap: 1rem;

  > div:last-child {
    .frm-in {
      > div {
        gap: 1rem;
      }
    }
  }

  .subscribers {
    gap: 0.25rem;

    .grid-column {
      align-items: center;

      .frm-in {
        margin: 0;
        padding: 0;
      }
    }

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

  .hide {
    display: none;
  }
`;
