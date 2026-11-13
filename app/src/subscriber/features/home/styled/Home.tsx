import styled from 'styled-components';

export const Home = styled.div`
  .folder-sub-menu {
    margin-left: auto;
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
      justify-content: center;
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
