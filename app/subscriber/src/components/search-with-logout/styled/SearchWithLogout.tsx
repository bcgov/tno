import styled from 'styled-components';
import { Row } from 'tno-core';

export const SearchWithLogout = styled(Row)`
  max-height: 4em;
  margin-bottom: 2.5%;
  width: 100%;
  justify-content: space-between;
  background-color: #ddd6c8;
  padding: 0.5em;
  align-items: center;
  box-shadow: 0 0.5em 0.5em -0.4em rgba(0, 0, 0, 0.2), 0 0 0 1px rgba(0, 0, 0, 0.02);
  .logout-icon {
    height: 1.5em;
    width: 1.5em;
    margin-bottom: 0.5em;
  }

  .frm-in {
    max-width: 10em;
  }
  .search {
    max-width: fit-content;
  }
  .logout {
    padding: 0.5em;
    max-height: fit-content;
    font-size: 1em;
    &:hover {
      cursor: pointer;
    }
    display: flex;
    svg {
      margin-right: 0.5em;
      margin-top: 0.25em;
    }
    color: ${(props) => props.theme.css.defaultRed};
  }
`;
