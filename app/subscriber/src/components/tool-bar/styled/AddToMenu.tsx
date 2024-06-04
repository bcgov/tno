import styled from 'styled-components';

export const AddToMenu = styled.div`
  .react-tooltip {
    font-size: 1.1em;
  }
  .section {
    font-style: italic;
    &:hover {
      color: ${(props) => props.theme.css.btnBkPrimary};
      font-weight: bold;
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
      .not-hovered {
        width: 18px;
      }
      .report-icon {
        display: none;
      }
      cursor: pointer;
      &:hover {
        color: ${(props) => props.theme.css.btnBkPrimary};
        font-weight: bold;
        .not-hovered {
          display: none;
        }
        .report-icon {
          display: block;
          width: 15px;
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
