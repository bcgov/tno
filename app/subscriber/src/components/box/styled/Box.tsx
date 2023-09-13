import styled from 'styled-components';

import { IBoxProps } from '../Box';

export const Box = styled.div.attrs<IBoxProps>(({ expand, canShrink }) => ({
  expand,
  canShrink,
}))<IBoxProps>`
  background-color: ${(props) => props.theme.css.backgroundColor};

  .box-header {
    color: ${(props) => props.theme.css.primaryColor};
    display: flex;
    flex-direction: row;
    gap: 0.5rem;
    align-items: center;
    padding: 0.25rem 0.5rem;
    border: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-top-left-radius: 0.25rem;
    border-top-right-radius: 0.25rem;
    background-color: ${(props) => props.theme.css.tableHeaderColor};

    h2 {
      text-transform: unset;
      margin: 0;
    }

    .box-header-title {
      cursor: ${(props) =>
        props.canShrink
          ? props.expand
            ? "url('/assets/caret-up-solid.svg'),pointer"
            : "url('/assets/caret-down-solid.svg'),pointer"
          : 'unset'};
    }
  }

  .box-body {
    padding: 0.5rem;
    border-left: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-right: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-bottom: solid 1px ${(props) => props.theme.css.lightVariantColor};
    border-bottom-left-radius: 0.25rem;
    border-bottom-right-radius: 0.25rem;
  }
`;
