import styled from 'styled-components';

export const ContentList = styled.div`
  width: 100%;
  .group-title {
    border-bottom: 1px solid ${(props) => props.theme.css.border};
  }
  .positive {
    color: ${(props) => props.theme.css.toneGreen};
  }
  .negative {
    color: ${(props) => props.theme.css.toneRed};
  }
  .neutral {
    color: ${(props) => props.theme.css.toneYellow};
  }
  .tone-icon {
    height: 20px;
    width: 20px;
    margin-right: 0.5rem;
  }
  .checkbox {
    margin-right: 0.5rem;
  }
  .date {
    font-family: ${(props) => props.theme.css.fPrimary};
    display: flex;
    margin-right: 1rem;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
  }
  .section {
    margin-left: auto;
  }
  .teaser {
    margin-left: 4.25rem;
    background-color: ${(props) => props.theme.css.teaserBackground};
    border-radius: 0.25rem;
    padding: 0.25rem;
    margin-bottom: 0.25rem;
    cursor: default;
  }
  .headline {
    background: none;
    border: none;
    color: blue;
    padding: 0;
    cursor: pointer;
    font-family: ${(props) => props.theme.css.fSecondary};
  }

  .headline:hover,
  .headline:focus {
    color: darkblue;
    outline: none;
  }
  .checked {
    background-color: ${(props) => props.theme.css.highlightYellow};
    .teaser {
      background-color: ${(props) => props.theme.css.highlightYellowDarker};
    }
  }
  .content-row {
    margin-bottom: 0.5rem;
    border-bottom: 1px solid ${(props) => props.theme.css.bkStaticGray};
    &:hover {
      background-color: ${(props) => props.theme.css.highlightYellow};
      .teaser {
        background-color: ${(props) => props.theme.css.highlightYellowDarker};
      }
    }
  }
`;
