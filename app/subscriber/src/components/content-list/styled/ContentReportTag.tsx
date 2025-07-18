import styled from 'styled-components';

export const ContentReportTag = styled.div`
  .tag-container {
    display: flex;
    flex-direction: row;
    align-items: center;
    padding: 0 0.5em;
    row-gap: 0.5em;
    justify-content: start;
    width: 90%;
    flex-wrap: wrap;
    margin: 0.5em 0 0.5em 3.5em;

    .report-tag {
      display: flex;
      flex-direction: row;
      align-items: center;
      border: solid 1px #c6c6c6;
      border-radius: 4px;
      padding: 0.2em 0.5em 0.2em 0.5em;
      margin: 0 0.5em 0 0;
    }
    .report-tag-text {
      text-overflow: ellipsis;
      max-width: 14em;
      white-space: nowrap;
      overflow: hidden;
      font-family: 'BC Sans';
      font-size: 0.8em;
      font-weight: 400;
      color: #41393b;
    }

    .tag-icon {
      height: 16px;
      width: 16px;
      pointer-events: none;
      padding-right: 0.3em;
    }

    .red {
      color: #eb8585;
    }

    .orange {
      color: #fdba74;
    }

    .yellow {
      color: #facc15;
    }

    .lime {
      color: #84cc16;
    }

    .emerald {
      color: #34d399;
    }

    .blue300 {
      color: #60a5fa;
    }

    .blue700 {
      color: #2563eb;
    }

    .indigo {
      color: #a5b4fc;
    }

    .violet {
      color: #a78bfa;
    }

    .fuschia {
      color: #f0abfc;
    }
  }
`;
