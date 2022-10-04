import styled from 'styled-components';
import { Row } from 'tno-core';

export const Breadcrumb = styled(Row)`
  & p:first-child {
    margin-right: 0.25em;
  }

  .clickable {
    color: ${(props) => props.theme.css.primaryLightColor};
    cursor: pointer;
    :after {
      content: '\u00A0>\u00A0';
      color: ${(props) => props.theme.css.textColor};
      font-weight: bold;
    }
  }
  .current {
    cursor: default;
  }

  svg {
    margin-top: 1%;
  }
`;
