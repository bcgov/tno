import styled from 'styled-components';

export const ReportSnapshot = styled.div`
  height: calc(100% + 32px - 1rem);

  h2 {
    margin: 0;
    text-transform: unset;

    &.ellipsis {
      max-width: 250px;
    }
  }

  .edit {
    flex: 1;

    > div {
      gap: 0.25rem;

      > div {
        gap: 0.25rem;
      }
    }

    .header {
      flex-wrap: nowrap;
      gap: 1rem;
      padding: 0.25rem;
    }

    .sections {
      .header {
        gap: 1rem;
        flex: 1 1 100%;
        align-items: center;
        overflow: hidden;
        max-width: 500px;
      }

      .section-container {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;

        .drag-row {
          border: solid 1px ${(props) => props.theme.css.lightVariantColor};
          border-radius: 0.25rem;
          background-color: white;
        }
      }
    }

    .edit-content {
      padding: 0 0 0 0.5rem;
      position: relative;

      .bottom-actions {
        padding: 0 0.5rem 0.5rem 0;
      }
    }

    .ql-editor {
      min-height: 12rem;
    }
  }

  .preview {
    flex: 1;
    border: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-radius: 0.25rem;

    .header {
      flex-wrap: nowrap;
      gap: 1rem;
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

  .loader {
    justify-content: center;
    position: relative;
    height: 100%;
  }
`;
