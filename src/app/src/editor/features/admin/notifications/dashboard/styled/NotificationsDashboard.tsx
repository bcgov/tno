import styled from 'styled-components';

export const NotificationsDashboard = styled.div`
  display: flex;
  flex-direction: column;
  padding: 1rem;
  height: 100%;

  .keyword-filter {
    margin-top: 1.5em;
  }

  .failed-filter {
    margin-top: 2em;
  }

  .header {
    display: grid;
    grid-template-columns: 1fr 2fr 2fr 1fr 1fr 50px;
    grid-column-gap: 0.25rem;

    > div {
      font-weight: 800;
    }
  }

  .filters {
    display: grid;
    grid-template-columns: 2fr 2fr 2fr 1fr 1fr;
    grid-column-gap: 0.5rem;
    white-space: nowrap;
  }

  .notifications {
    position: relative;
    flex: 1;
  }

  .notification-cards {
    display: grid;
    grid-template-columns: 1fr;
    grid-row-gap: 0.1rem;

    > div.notification-card {
      display: grid;
      grid-template-columns: 1fr 2fr 2fr 1fr 1fr 50px;
      grid-column-gap: 0.25rem;

      border: solid 1px ${(props) => props.theme.css.tableColor};
      border-radius: 0.25rem;
      background-color: white;
      box-shadow: 10px 5px 5px ${(props) => props.theme.css.tableColor};

      > div {
        padding: 0 0.25rem;

        .link {
          padding: unset;
        }
      }

      > div:first-child {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        > div {
          display: unset;
        }
      }

      .failed {
        border-color: ${(props) => props.theme.css.dangerColor};
        background-color: #d88c8f;
      }

      .buttons {
        display: flex;
        flex-direction: row;
        justify-content: flex-end;
        gap: 0.5rem;

        &.left {
          justify-content: flex-start;
        }

        > h3 {
          cursor: pointer;
          &:hover {
            text-decoration: none;
          }
        }

        > div {
          display: flex;
          flex-direction: row;
          align-content: center;
          align-items: center;
        }

        svg {
          cursor: pointer;
          vertical-align: unset;
          color: ${(props) => props.theme.css.iconPrimaryColor};

          &:hover {
            color: ${(props) => props.theme.css.iconPrimaryHoverColor};
          }
        }
      }

      .full-width {
        grid-column: 1 / -1;
        padding: 0.5rem;
        border-top: solid 1px ${(props) => props.theme.css.tableColor};
      }

      h3 {
        text-decoration: underline;
        padding: 0;
        margin: 0;
      }

      .response {
        border-bottom: solid 1px ${(props) => props.theme.css.tableColor};
        padding-bottom: 0.5rem;
        margin-bottom: 0.5rem;
      }

      .subscribers {
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        gap: 0.25rem;
        position: relative;

        > .subscriber {
          flex: 1;
          max-width: 500px;
          gap: 0.25rem;
          border: solid 1px ${(props) => props.theme.css.tableColor};
          border-radius: 0.25rem;
          padding: 0.25rem;
          position: relative;

          > div:first-child {
            display: flex;
            flex-direction: row;
            flex-wrap: nowrap;
            gap: 0.5rem;
          }

          .success {
            color: ${(props) => props.theme.css.completedColor};
          }

          .error {
            color: ${(props) => props.theme.css.dangerColor};
          }

          .response {
            background-color: ${(props) => props.theme.css.tableColor};
            padding: 0.5rem;
            border-radius: 0.5rem;
          }

          .notification-status {
            border: solid 1px ${(props) => props.theme.css.tableColor};
            border-radius: 0.5rem;
            padding: 0 0.25rem;
            cursor: pointer;
          }
        }
      }
    }
  }
`;
