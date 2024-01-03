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
    .txt-btn {
      margin: 0.25rem;
    }
  }
`;
