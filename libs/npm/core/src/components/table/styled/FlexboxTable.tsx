import styled from 'styled-components';

import { ITableStyleProps } from '..';

export const FlexboxTable = styled.div<ITableStyleProps>`
  display: flex;
  flex-flow: column;
  padding: 0.25em;

  .filter {
    display: flex;
    flex-flow: row;
    justify-content: center;
  }

  .header {
    display: flex;
    flex-flow: row;
    flex-wrap: nowrap;
    gap: 0.25em;
    border-top: solid 2px #000;
    border-bottom: solid 2px #000;
    margin-bottom: 0.25em;

    & .column {
      font-weight: bold;
      background: #fff;
      box-sizing: border-box;

      &:hover {
        filter: brightness(90%);
        border-radius: 0.25em;
      }

      & span {
        /* padding-right: 0.25em; */
      }

      & .sort {
      }

      & .sort:hover {
        color: lightblue;
        cursor: pointer;
      }
    }
  }

  .rows {
    overflow-y: auto;
    ${(props) =>
      props.scrollSize
        ? `height: ${isNaN(+props.scrollSize) ? props.scrollSize : `${props.scrollSize}px`};`
        : ''}

    & .row {
      display: flex;
      flex-flow: row;
      flex-wrap: nowrap;
      gap: 0.25em;
      background-color: #fff;
      padding: 0.25em 0;

      &:nth-child(even) {
        background-color: rgb(233, 236, 239);
      }

      &:hover {
        filter: brightness(75%);
        border-radius: 0.25em;
      }

      &.selected {
        background-color: rgb(66, 139, 202);
        border-radius: 0.25em;
        color: #fff;
      }

      &.active {
        background-color: rgb(56, 89, 138);
        border-radius: 0.25em;
        color: #fff;
      }
    }

    & .groups {
      & .group {
        background-color: rgb(119, 173, 223);
        padding: 0.25em;
        font-weight: bold;
        border-top-left-radius: 0.5em;
        border-top-right-radius: 0.5em;

        &:hover ~ .group-rows {
          filter: brightness(90%);
        }
      }
    }
  }

  .footer {
    display: flex;
    flex-flow: row;
    flex-wrap: nowrap;
    gap: 0.25em;
    justify-content: center;

    & .column {
      flex: 1;
    }
  }

  .pager {
    display: flex;
    flex-flow: row;
    gap: 1em;
    align-items: center;
    justify-content: center;
    padding: 0.25em;

    & .pages {
      display: flex;
      flex-flow: row;
      justify-content: center;
    }

    & .buttons {
      display: flex;
      flex-flow: row;
      justify-content: center;
      align-items: center;

      & .page-size {
        text-align: right;
        width: 3.5em;
        margin-left: 0.25em;
      }

      & .frm-in {
        padding: unset;
      }
    }
  }

  & .column {
    display: flex;
    flex-flow: row;
    gap: 0.5em;
    padding: 0.25em;
    box-sizing: border-box;

    &.col-0 {
      ${(props) => {
        const index = 0;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-1 {
      ${(props) => {
        const index = 1;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-2 {
      ${(props) => {
        const index = 2;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-3 {
      ${(props) => {
        const index = 3;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-4 {
      ${(props) => {
        const index = 4;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-5 {
      ${(props) => {
        const index = 5;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-6 {
      ${(props) => {
        const index = 6;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-7 {
      ${(props) => {
        const index = 7;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-8 {
      ${(props) => {
        const index = 8;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-9 {
      ${(props) => {
        const index = 9;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-10 {
      ${(props) => {
        const index = 10;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }

    &.col-11 {
      ${(props) => {
        const index = 11;
        if (props.columns.length > index) {
          const col = props.columns[index];
          return [
            `justify-content: ${col.hAlign ?? 'flex-start'};`,
            `align-content: ${col.vAlign ?? 'center'};`,
            col.width && isNaN(+col.width) ? `width: ${col.width};` : '',
            col.width && !isNaN(+col.width) ? `flex: ${col.width ?? '1'};` : '',
            !col.width ? ` flex: 1;` : '',
          ].join('\n');
        }
      }}
    }
  }
`;
