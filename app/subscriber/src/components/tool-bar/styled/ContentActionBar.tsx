import styled from 'styled-components';
import { Row } from 'tno-core';

export const ContentActionBar = styled(Row)`
  align-items: center;
  padding: 1em 0.5em 0;
  font-size: 0.9em;
  width: 100%;
  .left-side-items {
    margin-left: 0.5em;
    color: ${(props) => props.theme.css.btnBkPrimary};
    svg {
      margin-right: 0.5em;
    }
    &:hover {
      text-decoration: underline;
      cursor: pointer;
    }
  }

  .right-side-items {
    margin-left: auto;
    margin-right: 0.5em;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .action {
    margin-right: 1em;
    &:hover {
      text-decoration: underline;
      cursor: pointer;
    }
  }
`;
