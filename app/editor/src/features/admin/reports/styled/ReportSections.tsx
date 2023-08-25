import styled from 'styled-components';

export const ReportSections = styled.div`
  .options {
    border: solid 1px ${(props) => props.theme.css.beigeBackgroundColor};
    border-radius: 0.5rem;
    margin: 0.5rem 0 0 0;

    > label {
      border-top-left-radius: 0.5rem;
      border-top-right-radius: 0.5rem;
      padding: 0.5rem;
      background-color: ${(props) => props.theme.css.primaryLightColor};
      color: ${(props) => props.theme.css.backgroundColor};
    }

    > div {
      padding: 0.5rem;
    }
  }

  .section-table {
    display: grid;
    grid-template-columns: 1fr 2fr 5fr 1fr;
    row-gap: 0.25rem;
    width: 100%;

    .st-1 {
      grid-column-start: 1;
    }

    .st-2 {
      grid-column-start: 2;
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
    }

    .st-3 {
      grid-column-start: 3;
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
    }

    .st-4 {
      grid-column-start: 4;
      display: flex;
      flex-direction: row;
      justify-content: flex-end;
    }

    & > div {
      padding: 0.25rem;
    }

    .row-header {
      display: contents;
      & > div {
        font-weight: bold;
      }
    }

    .row {
      display: contents;
      cursor: pointer;

      & > div {
        display: flex;
        flex-flow: row nowrap;
        align-items: center;
        padding: 0.25rem 0.5rem;

        &:nth-child(1) {
          border-top-left-radius: 0.25rem;
          border-bottom-left-radius: 0.25rem;
        }

        &:nth-child(4) {
          border-top-right-radius: 0.25rem;
          border-bottom-right-radius: 0.25rem;
        }
      }

      &:nth-child(even) > div:nth-child(1),
      &:nth-child(even) > div:nth-child(2),
      &:nth-child(even) > div:nth-child(3),
      &:nth-child(even) > div:nth-child(4) {
        background-color: ${(props) => props.theme.css.tableEvenRowColor};
        border-top: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
        border-bottom: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
      }

      &:nth-child(odd) > div:nth-child(1),
      &:nth-child(odd) > div:nth-child(2),
      &:nth-child(odd) > div:nth-child(3),
      &:nth-child(odd) > div:nth-child(4) {
        background-color: ${(props) => props.theme.css.tableOddRowColor};
        border-top: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
        border-bottom: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
      }

      &:hover > div:nth-child(1),
      &:hover > div:nth-child(2),
      &:hover > div:nth-child(3),
      &:hover > div:nth-child(4) {
        background-color: ${(props) => props.theme.css.lightAccentColor};
      }
      &:hover > div:nth-child(4) {
        background-color: none;
      }

      &.active > div:nth-child(1),
      &.active > div:nth-child(2),
      &.active > div:nth-child(3),
      &.active > div:nth-child(4) {
        background-color: ${(props) => props.theme.css.activeColor};
        color: #fff;

        button {
          background-color: #fff;
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

    .section {
      display: block;
      grid-column: 1 /-1;
      background-color: ${(props) => props.theme.css.tableOddRowColor};
      border: solid 1px ${(props) => props.theme.css.tableEvenRowColor};
      border-bottom-left-radius: 0.5rem;
      border-bottom-right-radius: 0.5rem;
      padding: 1rem;
      box-shadow: 0 3px 10px rgba(0 0 0 / 0.5);
    }
  }
`;
