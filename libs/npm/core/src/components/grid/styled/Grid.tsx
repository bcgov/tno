import styled from 'styled-components';

import { IGridHeaderColumnProps } from '../Grid';

interface IGridProps {
  columns: (IGridHeaderColumnProps | undefined)[];
}

export const Grid = styled.div<IGridProps>`
  .grid-table {
    display: grid;
    grid-template-columns: ${(props) => props.columns.map((col) => col?.size ?? '1fr').join(' ')};
    row-gap: 0.5rem;

    .grid-header {
      border-top: solid 2px ${(props) => props.theme.css.linePrimaryColor};
      border-bottom: solid 2px ${(props) => props.theme.css.linePrimaryColor};
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-content: start;
      gap: 1rem;
      padding: 0 0.5rem;
      font-weight: 800;

      > svg {
        max-width: 12px;
        max-height: 12px;
        color: ${(props) => props.theme.css.iconPrimaryColor};

        &:hover:not(.disabled) {
          color: ${(props) => props.theme.css.linkPrimaryHoverColor};
          cursor: pointer;
        }
      }

      &:hover {
        background: ${(props) => props.theme.css.tableHoverRow};
      }
    }

    .grid-column {
      padding: 0.25rem 0.5rem;
      display: flex;
      flex-direction: row;
      gap: 1rem;
      border-bottom: solid 1px ${(props) => props.theme.css.tableEvenRow};
      align-items: center;
    }
  }

  .grid-pager {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    gap: 1rem;
    padding: 0.25rem;
    background: ${(props) => props.theme.css.tableEvenRow};
    border-bottom-left-radius: 1rem;
    border-bottom-right-radius: 1rem;

    > div {
      display: flex;
      flex-direction: row;
      align-items: center;

      svg {
        color: ${(props) => props.theme.css.iconPrimaryColor};

        &.disabled {
          color: ${(props) => props.theme.css.linkGrayColor};
        }

        &:hover:not(.disabled) {
          color: ${(props) => props.theme.css.linkPrimaryHoverColor};
          cursor: pointer;
        }
      }

      > div.frm-in {
        padding: 0;
        margin: 0;

        input {
          padding: 0 0.15rem;
        }
      }
    }
  }
`;
