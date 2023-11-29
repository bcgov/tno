import styled from 'styled-components';
import { Row } from 'tno-core';

export const BasicSearch = styled(Row)`
  width: 100%;
  margin: 0.25em;
  border-color: ${(props) => props.theme.css.inputGrey};
  border-style: solid;
  border-width: 1px;
  border-radius: 1em;
  background: ${(props) => props.theme.css.bkSecondary};
  box-shadow: ${(props) => props.theme.css.boxShadow};
  padding: 0.5em;

  /** SEARCH FOR TEXT */
  label {
    font-size: 0.8em;
    font-weight: 200;
    margin-right: 0.5em;
    align-self: center;
  }

  /** GROUP CONTAINING ICON AND SEARCH INPUT  */
  .icon-search {
    border: 0.5px solid ${(props) => props.theme.css.linePrimaryColor};
    border-radius: 1.5em;
    width: 30%;
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
  .search-input {
    input {
      outline: none;
      border: none;
      box-shadow: none;
      height: 2em;
    }

    width: 85%;
    margin-top: auto;
    margin-bottom: auto;
    padding: 0;
  }

  /* MOBILE SEARCH  */
  .search-mobile {
    display: flex;
    margin-top: auto;
    padding: 0;
  }
  /** SEARCH BUTTON */
  .search-button {
    display: flex;
    margin-left: 0.5em;
    align-self: center;
    font-weight: 200;
    font-size: 0.8em;
    height: 2em;
    /* border-radius: 1.5em; */
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    input&:focus {
      outline: none !important;
      box-shadow: none;
    }
    /* HIDE SVG IF SCREEN LESS THAN 750px */
    @media only screen and (max-width: 750px) {
      svg {
        display: none;
      }
    }
    svg {
      align-self: center;
      margin-left: 0.5em;
    }
  }

  /* BUTTON APPEAR ON RIGHT SIDE OF SEARCH IF SCREEN IS LESS THAN 750px */
  @media only screen and (max-width: 750px) {
    .search-button {
      margin-left: auto;
      margin-right: 0.5em;
    }
  }

  /* GO ADVANCED TEXT */
  p {
    font-size: 0.8em;
    font-weight: 200;
    margin-left: auto;
    align-self: center;
    margin-right: 0.5em;
    color: ${(props) => props.theme.css.fRedColor};
    &:hover {
      cursor: pointer;
      text-decoration: underline;
    }
  }

  /* CONDITIONALS TO HIDE THINGS FOR MOBILE/DESKTOP */

  /** HIDE SEARCH GROUP DISPLAY BASIC */
  @media only screen and (min-width: 750px) {
    .search-mobile {
      display: none;
    }
  }

  @media only screen and (max-width: 750px) {
    .icon-search {
      display: none;
    }
  }

  /* HIDE ADVANCED TEXT IF WIDTH GREATER THAN 750 */
  @media only screen and (max-width: 750px) {
    p {
      display: none;
    }
  }

  /* NO LABEL IF SCREEN SIZE LESS THAN 750px */
  @media only screen and (max-width: 750px) {
    label {
      display: none;
    }
  }
`;
