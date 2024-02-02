import styled from 'styled-components';

export const Home = styled.div`
  .folder-sub-menu {
    margin-left: auto;
  }
  /* table styling */
  .more-options {
    margin-left: 1em;
    margin-top: 0.25em;
    color: #6750a4;
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
    }
  }

  .show-media-label {
    font-weight: bold;
    margin-right: 1em;
    font-size: 0.85em;
    margin-top: 0.5em;
  }
  .filter-buttons {
    button {
      min-width: 5rem;
      border: none;
      display: flex;
      justify-content: center;
      &.active {
        background-color: ${(props) => props.theme.css.defaultRed};
        color: white;
      }
      &.inactive {
        background-color: ${(props) => props.theme.css.lightInactiveButton};
        color: #7a7978;
      }
    }
  }
  .fa-lg {
    color: ${(props) => props.theme.css.iconDarkColor};
    width: 27px;
    height: 18px;
    flex-shrink: 0;
  }

  .trancript-icon {
    color: ${(props) => props.theme.css.iconTertiaryColor};
  }
`;
