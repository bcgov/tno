import styled from 'styled-components';
import { Col } from 'tno-core';

export const SubscriberTableContainer = styled(Col)`
  width: 100%;
  .table {
    width: 100%;
    overflow: hidden;

    svg {
      margin-right: 0.5rem;
      height: 27px;
      min-height: 27px;
      max-height: 27px;
      flex-shrink: 0;
      &:hover {
        cursor: pointer;
        transform: scale(1.1);
      }
    }
    svg:not(.darker-icon) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }

    .darker-icon {
      color: ${(props) => props.theme.css.btnBkPrimary};
    }

    .row {
      font-size: 1em;
      font-weight: bold;
      color: ${(props) => props.theme.css.btnBkPrimary};
      &:hover {
        cursor: pointer;
      }
    }
    .header {
      color: ${(props) => props.theme.css.fPrimaryColor};
      font-size: 1em;
      border-top: none;
      border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    }
  }
`;
