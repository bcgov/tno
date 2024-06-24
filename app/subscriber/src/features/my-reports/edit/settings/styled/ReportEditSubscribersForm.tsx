import styled from 'styled-components';

export const ReportEditSubscribersForm = styled.div`
  display: flex;
  flex-direction: column;
  margin: 1rem;
  gap: 1rem;
  .select-column {
    width: 5rem !important;
  }
  .selected-all-btn {
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
    width: 10rem;
  }
    .select-all-btn {
    margin-top: 0.25rem;
    margin-bottom: 0.25rem;
    margin-left: 0.25rem;
    width: 2rem;
  }
  .subscriber-block {
    display: flex;
    gap: 0.5rem;
    align-items: stretch;
    .subscriber-title {
      font-weight: bold;
      font-size: 1.25rem;
    }
    .subscriber-describe {
      font-size: 1rem;
      margin-top: 0.8rem;
      margin-bottom: 0.8rem;
    }
    .report-exporter-container{
    }

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
