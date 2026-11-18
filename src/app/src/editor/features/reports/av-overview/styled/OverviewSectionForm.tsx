import styled from 'styled-components';

export const OverviewSectionForm = styled.div`
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
      background-color: #f5f5f5;
      padding: 0.25rem;
      display: flex;
      flex-direction: row;
      gap: 0.25rem;

      > div:first-child {
        min-width: 30px;
      }
      .placement-header,
      .summary-header,
      .content-header,
      .time-header {
        padding: 0 0.25rem;
      }
      .placement-header {
        min-width: 160px;
      }
      .time-header {
        min-width: 96px;
      }
      .content-header {
        min-width: 258px;
      }
      .delete-header {
        min-width: 16px;
      }
    }

    .contents {
      margin-top: 0.5em;

      .list-container {
        display: flex;
        flex-direction: column;
        gap: 0.15rem;
      }
    }

    .rows {
      display: flex;
      padding: 0.025rem 0.25rem;
      gap: 0.15rem;

      &:hover {
        background-color: #f5f5f5;
      }

      .frm-in {
        padding: 0;

        input {
          padding: 0;
        }

        textarea {
          resize: none;
        }
      }

      .frm-select {
        margin-right: 0;
      }

      .frm-time {
        > div {
          margin-right: 0;
          width: unset;
        }
      }
    }
  }
`;
