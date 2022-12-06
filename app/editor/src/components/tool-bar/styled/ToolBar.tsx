import styled from 'styled-components';
import { Row } from 'tno-core';

export const ToolBar = styled(Row)`
  background-color: ${(props) => props.theme.css.backgroundColor};
  max-height: 8.5em;
  align-items: center;
  border-radius: 4px;
  box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);

  .section {
    border-right: 1px solid #a8aab3;
  }
  .section:last-child {
    border-right: none;
  }
  .details-section {
    background-color: #faf9f7;
  }
`;
