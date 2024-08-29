import styled from 'styled-components';
import { Col } from 'tno-core';

export const MySearches = styled(Col)`
  .grid {
    margin-top: 0.75rem;
    .grid-header {
      border-top: none;
      border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    }
  }
  /* option items in the tooltip menu */
  .option {
    cursor: pointer;
    &:hover {
      text-decoration: underline;
    }
  }

  .grid-table.grid-header {
    display: none;
  }

  .search-row-options {
    display: flex;
    margin-left: auto;
    svg {
      outline: none;
      border: none;
      box-shadow: none;
    }
  }

  .row {
    .frm-in {
      padding-bottom: unset;
    }
    .txt-button {
      margin: 0.25rem;
    }
  }

  .keywords-row {
    display: flex;
    align-items: center;
    cursor: pointer;
  }
  .copy-icon {
    opacity: 0;
    transition: opacity 0.1s ease;
    margin-left: 1rem;
    cursor: pointer;
  }

  .copy-icon:hover {
    opacity: 1;
    color: ${(props) => props.theme.css.sideBarIconHoverColor};
  }
`;
