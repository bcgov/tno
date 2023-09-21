import styled from 'styled-components';

export const ReportAdmin = styled.div`
  display: flex;
  flex-direction: column;
  height: calc(100% + 50px);

  > div {
    height: 100%;
  }

  h2 {
    margin: 0;
    text-transform: unset;
  }

  form {
    height: 100%;
  }

  .report-name {
    max-height: 42px;
    overflow: hidden;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;

    h1 {
      text-transform: unset;
      margin: 0;
    }
  }

  .btn.danger {
    color: darkred;

    &:hover {
      color: red;
    }
  }

  .report-layout {
  }

  .report-sections {
    /* overflow-y: scroll; */
    margin-top: 1rem;

    > div {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      /* height: calc(100vh - 450px); */
      /* max-height: 100%; */
      break-before: column;
    }
  }

  .schedules {
    padding: 0.5rem;
    .schedule {
      justify-content: center;
      background-color: lightgrey;
      border-radius: 0.5rem;
      padding: 0.5rem;
    }
  }

  .draggable {
    cursor: grab;
  }

  .spinner {
    color: white;
  }

  .charts {
    gap: 0.25rem;

    > div {
      padding: 0.25rem;
      border-radius: 0.25rem;
    }
    > div:nth-child(odd) {
      background-color: ${(props) => props.theme.css.tableOddRowColor};
    }
    > div:nth-child(even) {
      background-color: ${(props) => props.theme.css.tableEvenRowColor};
    }
  }

  .tab-menu {
    > div {
      > div {
        margin-top: 0.25rem;

        > div:nth-child(3) {
          margin-bottom: 0.25rem;
        }
      }
    }
  }

  .tab-container {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .preview {
    flex: 1;
    border: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-radius: 0.25rem;

    .header {
      flex-wrap: nowrap;
      padding: 0.25rem;

      > div {
        gap: 1rem;
        align-items: center;

        &:nth-child(1) {
          padding-left: 1rem;
        }

        &:nth-child(2) {
          flex-wrap: nowrap;
          flex: 1;
          justify-content: flex-end;
          min-width: 250px;
        }
      }

      .frm-in {
        padding: unset;
      }
    }

    .preview-report {
      .diagram {
        padding: 1rem;
        gap: 1rem;

        > div {
          gap: 1rem;

          > div:first-child {
            min-width: 100px;
            border: solid 1px ${(props) => props.theme.css.activeColor};
            border-radius: 0.5rem;
            padding: 0.5rem;
          }

          h3 {
            margin: unset;
          }
        }

        svg {
          color: ${(props) => props.theme.css.activeColor};
          padding: 0.5rem;
          margin: 0.5rem;
        }
      }

      > .preview-header {
        flex: 1;
        gap: 1rem;
        align-items: center;
        padding: 0.25rem;
        border-top: solid 1px ${(props) => props.theme.css.lightVariantColor};
        border-bottom: solid 1px ${(props) => props.theme.css.lightVariantColor};
        background-color: ${(props) => props.theme.css.tableHeaderColor};

        > .preview-subject {
          max-width: calc(100% - 2rem);
        }
      }

      > .preview-body {
        flex: 1;
        max-width: calc(100% - 2rem);
        padding: 0.5rem;

        img {
          max-width: 80%;
        }
      }
    }
  }
`;
