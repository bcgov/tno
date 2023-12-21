import styled from 'styled-components';
import { Row } from 'tno-core';

export const SearchWithLogout = styled(Row)`
  min-height: 3em;
  border-bottom: 1px solid ${(props) => props.theme.css.linePrimary};
  padding: 0.5rem 2rem 0.5rem 0.5rem;
  background-color: ${(props) => props.theme.css.bkMain};
  align-items: center;

  .logout-icon {
    height: 1.5em;
    width: 1.5em;
    margin-bottom: 0.5em;
  }

  .frm-in {
    padding-right: 0;
  }
  .logo-container {
    width: 15em;
    .mm-logo {
      width: 100%;
    }
  }
`;
