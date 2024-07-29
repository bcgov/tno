import styled from 'styled-components';
import { Col } from 'tno-core';

export const MySearches = styled(Col)`
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
    padding: 0.5rem;
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
