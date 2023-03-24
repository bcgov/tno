import styled from 'styled-components';
import { Row } from 'tno-core';

export const Landing = styled(Row)`
  overflow: hidden;
  font-family: -apple-system, BlinkMacSystemFont, 'BCSans', 'Noto Sans', Arial, 'sans serif';
  display: flex;

  /* The panel containing Commentary and front pages */
  .right-panel {
    margin-left: 1.5em;
    .title {
      background-color: ${(props) => props.theme.css.subscriberHeaderColor};
      padding: 0.5em;
      font-size: 1.25em;
      color: white;
    }
    .commentary {
      margin-top: 5em;
      width: 100%;
      .content {
        background-color: white;
        min-height: 15em;
      }
    }
    display: flex;
    .header {
      padding: 0.5em;
      max-height: fit-content;
      justify-content: space-between;
      flex-grow: 1;
      .search {
        max-width: 20em;
      }
    }
    .logout {
      padding: 0.5em;
      max-height: fit-content;
      font-size: 1.5em;
      &:hover {
        cursor: pointer;
      }
      display: flex;
      svg {
        margin-right: 0.5em;
        margin-top: 0.25em;
      }
      color: ${(props) => props.theme.css.subscriberRed};
    }
    min-width: 39%;
    background-color: #black;
    input {
      min-height: 3em;
      border-radius: none;
      max-width: 60%;
    }
  }

  /* The panel containing the media list */
  .main-panel {
    min-width: 59%;
    .show-media-label {
      align-self: center;
      font-weight: bold;
      margin-right: 1em;
    }

    .filter-buttons {
      margin-top: 2.5em;
    }
    margin-left: 0.5%;
    .title {
      background-color: ${(props) => props.theme.css.subscriberHeaderColor};
      padding: 0.5em;
      font-size: 1.75em;
      color: white;
    }
    .content {
      background-color: ${(props) => props.theme.css.lightGray};
      padding: 1em;
      min-height: 45em;
      .date-navigator {
        .calendar {
          color: #3847aa;
        }
        svg {
          align-self: center;
          height: 1.5em;
          width: 1.5em;
          &:hover {
            cursor: pointer;
          }
        }
        margin-bottom: 1em;
      }
    }

    /* TODO: move these to the button component as styling configuration */
    button {
      border-radius: 1.25rem;
      margin-bottom: 10%;
      min-width: 5rem;
      border: none;
      display: flex;
      justify-content: center;
      &.active {
        background-color: ${(props) => props.theme.css.subscriberRed} !important;
        color: white;
      }
      &.inactive {
        background-color: ${(props) => props.theme.css.subscriberInactiveButton} !important;
        color: #7a7978;
      }
    }
  }
`;

export default Landing;
