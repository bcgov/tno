import styled from 'styled-components';

import { ISectionProps } from '../Section';

export const Section = styled.div<ISectionProps>`
  display: flex;
  flex-direction: column;
  border-bottom-right-radius: 0.25rem;
  border-bottom-left-radius: 0.25rem;
  color: ${(props) => props.theme.css.fPrimaryColor};
  box-shadow: ${(props) => (props.open ? props.theme.css.boxShadow : 'unset')};

  .section-header {
    display: flex;
    flex-direction: row;
    gap: 1rem;
    align-items: center;
    padding: 0.25rem 0.5rem;
    background-color: ${(props) => props.theme.css.bkTertiary};
    border-top-right-radius: 0.25rem;
    border-top-left-radius: 0.25rem;
    border-bottom-right-radius: ${(props) => (!props.open ? '0.25rem' : 'unset')};
    border-bottom-left-radius: ${(props) => (!props.open ? '0.25rem' : 'unset')};

    .section-active {
      width: 6px;
      background-color: ${(props) => props.theme.css.highlightActive};
    }

    .section-icon {
      display: flex;
      flex-direction: row;
      align-items: center;
      justify-items: center;

      svg {
        color: ${(props) => props.theme.css.iconPrimaryColor};
        height: 25px;
        min-height: 25px;
        max-height: 25px;
        width: 25px;
        min-width: 25px;
        max-width: 25px;
        flex-shrink: 0;
      }
    }

    .section-label {
      flex: 1;
      overflow: hidden;
      display: block;
      white-space: nowrap;
      cursor: ${(props) => (props.showOpen ? 'pointer' : 'unset')};
    }

    .section-actions {
      display: flex;
      flex-direction: row;
      flex-wrap: nowrap;
      gap: 1rem;
    }

    .section-open {
      padding-left: 1rem;
    }
  }

  .section-body {
    display: flex;
    flex-direction: column;
    padding: 0.5rem 1rem;
  }
`;
