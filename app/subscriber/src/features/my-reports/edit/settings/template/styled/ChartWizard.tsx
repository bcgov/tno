import styled from 'styled-components';

export const ChartWizard = styled.div`
  flex: 1;
  display: flex;
  flex-direction: row;
  gap: 1rem;

  > div:first-child {
    display: flex;
    flex-direction: row;
    width: 500px;

    ol {
      list-style-type: none;
      margin: 0;
      padding: 0;

      li {
        background-color: ${(props) => props.theme.css.tableHoverRow};
        padding: 0.15rem 0.5rem;
        margin: 0.15rem 0;
        border-top-left-radius: 0.5rem;
        border-bottom-left-radius: 0.5rem;

        &:hover {
          color: ${(props) => props.theme.css.tableActiveRowColor};
          background-color: ${(props) => props.theme.css.tableActiveRow};
          cursor: pointer;
        }

        &.active {
          color: ${(props) => props.theme.css.tableActiveRowColor};
          background-color: ${(props) => props.theme.css.tableActiveRow};
        }
      }
    }

    > div:last-child {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
  }

  > div:last-child {
    flex: 1;
    display: flex;
    flex-direction: column;
    background-color: ${(props) => props.theme.css.tableHoverRow};
    border-radius: 0.5rem;
    padding: 0.25rem;
    justify-content: center;
    align-items: center;
    overflow: auto;
    position: relative;

    canvas {
      display: block;
      background-color: white;
      border-radius: 0.5rem;
    }
  }

  .chart-config {
    > div:last-child {
      border: solid 3px ${(props) => props.theme.css.tableActiveRow};
      border-top-right-radius: 0.5rem;
      border-bottom-right-radius: 0.5rem;
      padding: 0.5rem;
    }

    .chart-config-action-bar {
      background-color: ${(props) => props.theme.css.tableHoverRow};
      padding: 0.25rem;
    }

    .chart-config-window {
      min-height: 200px;
      max-height: 250px;
      overflow-y: auto;
      padding-right: 1rem;
    }
  }
`;
