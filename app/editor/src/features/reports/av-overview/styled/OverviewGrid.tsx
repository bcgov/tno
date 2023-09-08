import styled from 'styled-components';

export const OverviewGrid = styled.div`
  .no-items {
    margin-left: auto;
    margin-right: auto;
    max-width: fit-content;
  }
  .grid {
    border: 1px solid #f5f5f5;
    box-shadow: 0 0 5px 0 rgba(0, 0, 0, 0.2);
    margin-left: auto;
    margin-right: auto;

    .summary {
      resize: vertical;
      max-height: fit-content;
    }
    .grip-lines {
      margin-right: 0.5em;
      margin-left: 0.5em;
      align-self: center;
      &:hover {
        color: ${(props) => props.theme.css.primaryLightColor};
      }
    }
    .clear-item {
      margin-top: 0.75em;
      cursor: pointer;
      &:hover {
        color: #ff0000;
      }
    }
    .header {
      font-weight: bold;
      width: 100%;
      background-color: #f5f5f5;
    }
    .time-header {
      width: 9em;
    }
    .placement-header {
      width: 11em;
    }

    .summary-header {
      max-width: 65em;
    }
    .placement-header,
    .summary-header,
    .content-header,
    .time-header {
      padding: 0.75em;
    }
    .delete-header {
      min-width: 16px;
    }

    .contents {
      margin-top: 0.5em;
    }

    .rows {
      &:hover {
        background-color: #f5f5f5;
      }
      padding: 0.5em;
    }
  }

  .item-container {
    display: flex;
    flex-direction: column;
  }
`;
