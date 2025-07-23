import styled from 'styled-components';
import { Col } from 'tno-core';

export const MyProducts = styled(Col)`
  .info {
    margin-top: 1em;
    margin-bottom: 1em;
  }

  .modal-select {
    width: 80%;
  }

  .product-container {
    svg {
      margin-right: 0.5em;
    }
    .action {
      label {
        min-width: 6.5em;
      }
    }
    h2 {
      display: flex;
    }
    .info-text {
      margin-top: 0.5em;
      margin-bottom: 0.5em;
    }
    padding: 1em;
    .product-card {
      border-top: 1px solid ${(props) => props.theme.css.iconGrayColor};
      border-bottom: 1px solid ${(props) => props.theme.css.iconGrayColor};
      padding: 0.5em 0 0.5em 0;
    }
    .product-description {
      background-color: ${(props) => props.theme.css.teaserBackground};
      border-radius: 0.5em;
      padding: 0.5em;
      margin-top: 0.5em;
      margin-bottom: 0.5em;
    }
    svg {
      align-self: center;
    }
    .product-section-title {
      margin: 0;
      font-size: 1.5em;
      color: ${(props) => props.theme.css.darkHeaderColor};
      border-bottom: 1px solid ${(props) => props.theme.css.darkHeaderColor};
      product-icon {
        margin-right: 0.5em;
      }
    }
    .product-section-title:not(:first-child) {
      margin-top: 1em;
    }
    .product-row {
      color: ${(props) => props.theme.css.linkPrimaryColor};
      svg {
        height: 1.25em;
        width: 1.25em;
      }
    }
    .product-name {
      font-size: 1.25em;
      margin-right: auto;
    }
  }

  .header {
    .create-new {
      margin-left: auto;
      color: red;
      &:hover {
        text-decoration: underline;
        cursor: pointer;
      }
    }
  }

  .react-tooltip {
    z-index: 999;
  }

  .options {
    box-shadow: 0 0 0.5rem #c7c7c7;
    opacity: 1;
    padding-left: 1.5em;
    padding-right: 1.5em;
    .option {
      &:hover {
        text-decoration: underline;
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        cursor: pointer;
      }
    }
  }
  .action-subscribe > svg,
  .action-subscribe > label {
    color: ${(props) => props.theme.css.btnBkSuccess};
  }
  .action-unsubscribe > svg,
  .action-unsubscribe > label {
    color: ${(props) => props.theme.css.btnRedColor};
  }
  .folder-name {
    height: 1.5em;
    margin-left: 0.5em;
  }

  .create-new {
    margin-left: auto;
  }

  /* table styling */
  .table {
    width: 100%;
    overflow: hidden;
    .re-name {
      height: 1.5em;
      padding: 0;
      &:focus {
        box-shadow: none;
      }
    }
    .rows {
      svg:not(.error) {
        color: ${(props) => props.theme.css.btnBkPrimary};
      }
    }
    .header {
      font-size: 0.8em;
      /* box shadow only on bottom */
      box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.1);
      border: none;
      color: ${(props) => props.theme.css.fPrimaryColor};
    }
    .elips {
      &:hover {
        color: ${(props) => props.theme.css.sidebarIconHoverColor};
        transform: scale(1.1);
      }
    }
  }

  .folder-add {
    &:focus {
      outline: none;
    }
    cursor: pointer;
    align-self: center;
    margin-bottom: 0.5rem;
    height: 1.6rem;
    width: 2.5rem;
    color: ${(props) => props.theme.css.sidebarIconColor};
    padding-bottom: 0.25em;
    &:hover {
      transform: scale(1.1);
      color: ${(props) => props.theme.css.sidebarIconHoverColor};
    }
  }

  .table {
    min-height: 100px;
  }
`;
