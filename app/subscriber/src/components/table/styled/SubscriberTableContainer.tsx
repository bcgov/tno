import styled from 'styled-components';
import { Col } from 'tno-core';

export const SubscriberTableContainer = styled(Col)`
  width: 100%;
  overflow: hidden;

  .header {
    border-top: none;
    border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
  }

  .label {
    font-weight: 700;
  }

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

  svg {
    &.darker-icon {
      color: ${(props) => props.theme.css.btnBkPrimary};
    }

    &:not(.darker-icon) {
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
  }

  .row {
    &:hover {
      cursor: pointer;
    }
    font-weight: bold;
    color: ${(props) => props.theme.css.btnBkPrimary};
    gap: 0.25rem;
    padding: 0.25rem 0.25rem;
    align-items: center;

    background: ${(props) => props.theme.css.bkWhite};

    // rather than updating the tno-core package and adding props to control the zebra striping of the table with more props, we'll just override the css here
    // as this wrapper will be applied to most tables in the subscriber app.
    &:nth-child(2n) {
      background: inherit !important;
    }

    &:hover {
      background: ${(props) => props.theme.css.tableHoverRow};
      filter: brightness(95%);
      border-radius: 0.25rem;
    }

    .link:hover {
      cursor: pointer;
    }

    &.active:not(:hover) {
      color: ${(props) => props.theme.css.tableActiveRowColor};
      background: ${(props) => props.theme.css.tableActiveRow};

      svg {
        &.darker-icon {
          color: ${(props) => props.theme.css.iconSecondaryColor};
        }

        &:not(.darker-icon) {
          color: ${(props) => props.theme.css.iconSecondaryColor};
        }
      }
    }
  }
`;
