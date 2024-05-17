import styled from 'styled-components';
import { Row } from 'tno-core';

export const BasicSearch = styled(Row)<{ inHeader?: boolean }>`
  @media only screen and (max-width: 900px) {
    border-top: 1px solid ${(props) => props.theme.css.linePrimaryColor};
    padding: 0.25em;
    border-bottom: 1px solid ${(props) => props.theme.css.linePrimaryColor};
  }
  ${(props) =>
    props.inHeader &&
    `
      width: unset;
    `}
  ${(props) =>
    !props.inHeader &&
    `
 @media only screen and (min-width: 900px) {
      background: ${props.theme.css.bkSecondary};
      margin: 0.25em;
      border-color: ${props.theme.css.inputGrey};
      margin-bottom: 1em;
      border-style: solid;
      border-width: 1px;
      border-radius: 1em;
      box-shadow: ${props.theme.css.boxShadow};
      padding: 0.5em;
 }
  `}
  align-items: center;
  flex: 1;

  /** SEARCH FOR TEXT */
  label {
    text-transform: uppercase;
    font-size: 1em;
    margin-right: 0.5em;
    align-self: center;
    margin-left: ${(props) => props.inHeader && '5%'};
  }

  /** GROUP CONTAINING ICON AND SEARCH INPUT  */
  @media only screen and (max-width: 900px) {
    width: '';
  }
  .icon-search {
    border: 0.5px solid ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 1.5em;
    background-color: ${(props) => props.theme.css.bkWhite};
    height: 2.5em;
  }

  .search-icon {
    margin: 0.2em;
    margin-left: 0.3em;
    align-self: center;
    color: ${(props) => props.theme.css.iconSecondaryColor};
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
    }
  }

  .search-row {
    flex-flow: nowrap;
    width: 100%;
  }

  .search-input {
    input {
      outline: none;
      border: none;
      box-shadow: none;
      height: 2em;
    }

    width: calc(90% - 1rem);
    margin-top: auto;
    margin-bottom: auto;
    padding: 0;
  }

  /** SEARCH BUTTON */
  .search-button {
    display: flex;
    margin-left: 0.5em;
    align-self: center;
    font-weight: 400;
    font-size: 0.8em;
    height: 2em;
    /* border-radius: 1.5em; */
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    input&:focus {
      outline: none !important;
      box-shadow: none;
    }
    /* HIDE SVG IF SCREEN LESS THAN 900px */
    @media only screen and (max-width: 900px) {
      svg {
        display: none;
      }
    }
    svg {
      align-self: center;
      margin-left: 0.5em;
    }
  }

  /* BUTTON APPEAR ON RIGHT SIDE OF SEARCH IF SCREEN IS LESS THAN 900px */
  @media only screen and (max-width: 900px) {
    .search-button {
      margin-right: 0.5em;
    }
  }

  /* GO ADVANCED TEXT */
  .go-advanced {
    font-size: 1em;
    margin-left: ${(props) => (props.inHeader ? '3em' : 'auto')};
    align-self: center;
    margin-right: 0.5em;
    color: ${(props) => props.theme.css.fRedColor};
    text-decoration: none;
    text-transform: uppercase;
    &:hover {
      cursor: pointer;
      text-decoration: underline;
    }
  }

  /* CONDITIONALS TO HIDE THINGS FOR MOBILE/DESKTOP */

  @media only screen and (max-width: 900px) {
    .mobile-search-input {
      padding-bottom: 0;
    }
  }

  /* HIDE ADVANCED TEXT IF WIDTH GREATER THAN 750 */
  @media only screen and (max-width: 900px) {
    p {
      display: none;
    }
  }

  /* NO LABEL IF SCREEN SIZE LESS THAN 900px */
  @media only screen and (max-width: 900px) {
    label {
      display: none;
    }
  }
`;
