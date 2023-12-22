import styled from 'styled-components';
import { Col } from 'tno-core';

export const Navbar = styled(Col)<{ $expanded: boolean }>`
  height: 100dvh;
  svg:not(.expand-control) {
    height: ${(props) => (props.$expanded ? '' : '1.5em')};
    width: ${(props) => (props.$expanded ? '' : '1.5em')};
  }
  background-color: ${(props) => props.theme.css.bkMain};
  .expand-control {
    margin-top: 3em;
    margin-left: ${(props) => (props.$expanded ? 'auto' : '1em')};
  }
  .option {
    max-width: ${(props) => (props.$expanded ? '' : 'fit-content')};
    padding: 0.5em;
    cursor: pointer;
    align-items: center;
    background-color: ${(props) => props.theme.css.navItemBackgroundColor};
    margin-bottom: 0.1em;
    color: ${(props) => props.theme.css.bkWhite};
    align-items: center;
    svg {
      margin-right: 0.5em;
    }
  }
  .my-content {
    background-color: ${(props) => props.theme.css.navItemSecondaryBackgroundColor};
    .secondary-icon {
      margin-left: auto;
    }
  }
  .group-title {
    color: ${(props) => props.theme.css.iconTertiaryColor};
    svg {
      margin-right: 0.5em;
      margin-left: 0.5em;
      margin-top: 0.5em;
    }
  }
`;
