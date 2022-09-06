import styled from 'styled-components';
import { Row } from 'tno-core';

export const Breadcrumb = styled(Row)`
  .clickeable {
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
