import styled from 'styled-components';
import { Col } from 'tno-core';

export const SubscriberTableContainer = styled(Col)`
  width: 100%;
  overflow: hidden;

  .header {
    color: ${(props) => props.theme.css.fPrimaryColor};
    font-size: 1em;
    border-top: none;
    border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    margin-bottom: 0.5rem;
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
    font-size: 1em;
    font-weight: bold;
    color: ${(props) => props.theme.css.btnBkPrimary};
    gap: 0.25rem;
    padding: 0.25rem 0.25rem;
    align-items: center;

    &:nth-child(odd) {
      background: ${(props) => props.theme.css.tableOddRow};
    }

    &:nth-child(even) {
      background: ${(props) => props.theme.css.tableEvenRow};
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
