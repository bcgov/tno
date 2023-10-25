import styled from 'styled-components';

export const TopicScoreRuleList = styled.div`
  width: auto;
  height: 100%;
  min-height: 100%;
  display: flex;
  justify-content: center;

  .filter-bar {
    display: flex;
    align-items: center;
    input {
      margin-top: 3.5%;
    }
    button {
      background-color: white;
    }
    background-color: #f5f5f5;
  }

  .form-footer {
    // It overlaps the bottom row and it's impossible to scroll further.
    /* position: fixed; */
    bottom: 0;
    width: 100%;
    background-color: ${(props) => props.theme.css.backgroundColor};
    z-index: 2;
  }

  .grid-table {
    display: grid;
    grid-template-columns: 3fr 3fr 2fr repeat(9, 1fr);
    align-items: center;
    grid-gap: 0.25em;
    min-height: 200px;

    > div {
      &.center {
        justify-self: center;
      }

      .text-right {
        text-align: right;
      }
    }

    .grid-row {
      display: contents;

      > div {
        .frm-in {
          padding: 0;
        }

        &.center {
          justify-self: center;
        }
      }

      button.move {
        height: 40%;

        padding: 0;
        margin-right: 0.5em;

        svg {
          height: 100%;
        }
      }
    }

    .add-row {
      grid-column: 1/13;
      background-color: ${(props) => props.theme.css.borderColor};
      height: 5px;
      align-content: center;
      justify-content: center;
      z-index: 2;

      &:hover {
        background-color: ${(props) => props.theme.css.lightVariantColor};
        cursor: pointer;

        svg {
          background-color: white;
          color: ${(props) => props.theme.css.activeColor};
          border-radius: 1em;
        }
      }
    }
  }
`;
