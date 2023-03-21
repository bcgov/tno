import styled from 'styled-components';

import { Row } from '../../flex';
import { IToolBarProps } from '../ToolBar';

export const ToolBar = styled(Row)<IToolBarProps>`
  background-color: ${(props) =>
    props.variant === 'dark'
      ? props.theme.css.darkerBackgroundColor
      : props.theme.css.backgroundColor};
  align-items: center;
  border-radius: 4px;
  box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
  justify-content: center;

  .section {
    border-right: 1px solid #a8aab3;
  }
  .section:last-child {
    border-right: none;
  }
  margin-bottom: 0.5em;
`;
