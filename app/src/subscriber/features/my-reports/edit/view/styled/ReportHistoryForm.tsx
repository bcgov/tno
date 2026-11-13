import styled from 'styled-components';

export const ReportHistoryForm = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;
  div.report-history {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr 1fr;
    .report-status {
      color: ${(props) => props.theme.css.fInfo};
    }

    > div {
      padding: 0.25rem 1rem;
    }

    > div:nth-child(8n + 1),
    > div:nth-child(8n + 2),
    > div:nth-child(8n + 3),
    > div:nth-child(8n + 4) {
      background: ${(props) => props.theme.css.highlightSecondary};
    }

    > div:nth-child(1),
    > div:nth-child(2),
    > div:nth-child(3),
    > div:nth-child(4) {
      background: ${(props) => props.theme.css.sectionHeader};
      color: ${(props) => props.theme.css.sectionHeaderText};
      font-weight: 800;
    }

    > div.col-1 {
      min-width: 12rem;
    }
    > div.col-3 {
      min-width: 12rem;
    }

    > div.col-4 {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
    }
  }
`;
