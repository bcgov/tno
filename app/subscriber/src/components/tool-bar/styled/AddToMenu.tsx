import styled from 'styled-components';

export const AddToMenu = styled.div`
  @keyframes spin {
    0% {
      transform: rotate(0deg);
    }
    100% {
      transform: rotate(360deg);
    }
  }
  .report-icon {
    position: absolute;
    left: 1em;
    width: 15px;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .report-name {
    margin-left: 1.25em;
  }
  .react-tooltip {
    font-size: 1.1em;
  }
  .section {
    font-style: italic;
    .spinner {
      margin-left: auto;
      animation: spin 1s linear infinite;
    }

    &:hover {
      color: ${(props) => props.theme.css.btnBkPrimary};
      font-weight: bold;
      border-bottom: 1px solid ${(props) => props.theme.css.btnBkPrimary};
    }
    .active-section {
      position: absolute;
      left: 2.85em;
    }
    margin-left: 3em;
    &:not(:hover) {
      .active-section {
        display: none;
      }
    }
  }
  .choose-report {
    border-bottom: 1px solid ${(props) => props.theme.css.btnGrayColor};
    margin-bottom: 0.5em;
    font-weight: bold;
  }
  .list {
    .row-title {
      font-weight: bold;
    }
    .report-item {
      font-weight: 600;
      .report-icon:not(.expanded) {
        display: none;
      }
      cursor: pointer;
      &:hover {
        color: ${(props) => props.theme.css.btnBkPrimary};
        font-weight: bold;
        .report-icon {
          display: block;
        }
      }
      min-width: 15em;
    }
    .expand-sections {
      margin-left: auto;
      align-self: center;
      color: ${(props) => props.theme.css.iconPrimaryColor};
    }
  }
`;
