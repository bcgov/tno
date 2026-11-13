import styled from 'styled-components';

export const Impersonation = styled.div`
  .bar {
    > div {
      flex: 1;
      justify-content: space-between;
      align-items: center;
    }
  }

  .table {
    padding: 0.25rem 1rem;

    .table-header {
      font-weight: 800;
      border-bottom: solid 1px ${(props) => props.theme.css.tableEvenRow};
    }

    > div.table-rows {
      > div {
        padding: 0.5rem;
      }
      > div:nth-child(odd) {
        background: ${(props) => props.theme.css.tableOddRow};
      }
      > div:nth-child(even) {
        background: ${(props) => props.theme.css.tableEvenRow};
      }

      > div.active {
        background: ${(props) => props.theme.css.tableActiveRow};
        color: ${(props) => props.theme.css.tableActiveRowColor};
        svg {
          color: ${(props) => props.theme.css.tableActiveRowColor};
        }
      }
    }

    .table-footer {
      align-items: center;
      justify-content: center;
      border-top: solid 1px ${(props) => props.theme.css.tableEvenRow};
      border-bottom: solid 1px ${(props) => props.theme.css.tableEvenRow};
      padding: 0.5rem 0;

      .frm-in {
        padding: unset;

        input {
          text-align: right;
        }
      }
    }
  }

  input[type='number'] {
    appearance: textfield;
    margin: 0;
  }
`;
