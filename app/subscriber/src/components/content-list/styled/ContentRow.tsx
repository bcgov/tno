import styled from 'styled-components';
import { Col } from 'tno-core';

export const ContentRow = styled(Col)`
  margin-bottom: 0.5rem;
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
  border-bottom: 1px solid ${(props) => props.theme.css.bkStaticGray};

  .icon-row {
    margin-left: 2rem;
    min-width: 75px;
  }

  .has-divider {
    position: relative;
    margin: 0 5px;
  }

  .icon-placeholder {
    width: 20px;
    height: 20px;
  }

  .add-margin {
    margin-left: 4rem;
  }

  .pin-icon {
    &:hover {
      cursor: pointer;
    }
  }

  .attributes {
  }

  @media (max-width: 768px) {
    .attributes:not(.mobile) {
      display: none;
    }
  }

  @media (min-width: 768px) {
    .mobile {
      display: none;
    }
  }

  .transcript-feather {
    height: 20px;
    width: 20px;
    margin-right: 0.5rem;
  }

  .popout-placeholder {
    width: 20px;
    height: 20px;
    margin-left: 0.5rem;
    margin-right: 0.5rem;
  }

  .section,
  .series,
  .source,
  .byline {
    margin-left: 5px;
  }

  .has-divider::after {
    content: '';
    position: absolute;
    right: -5px; /* adjust to control the spacing between text and divider */
    top: 50%;
    transform: translateY(-60%);
    width: 1px;
    height: 14px;
    background-color: black;
  }

  .parent-row {
    flex-flow: nowrap;
    width: 100%;
  }
  &:hover {
    background-color: ${(props) => props.theme.css.highlightYellow};
    .teaser {
      background-color: ${(props) => props.theme.css.highlightYellowDarker};
    }
  }
  &.checked {
    background-color: ${(props) => props.theme.css.highlightYellow};
    .teaser {
      background-color: ${(props) => props.theme.css.highlightYellowDarker};
    }
  }

  .media-playback {
    video {
      height: 270px;
      width: 480px;
      margin-left: auto;
      margin-right: auto;
    }
    audio {
      margin-left: auto;
      margin-right: auto;
    }
    .copyright-text {
      margin-top: 0.5rem;
      font-size: 0.9rem;
      display: flex;
      margin-left: auto;
      margin-right: auto;
      color: ${(props) => props.theme.css.iconGrayColor};
      svg {
        margin-right: 0.25rem;
      }
    }
  }
  .positive {
    color: ${(props) => props.theme.css.tonePositive};
  }
  .negative {
    color: ${(props) => props.theme.css.toneNegative};
  }
  .neutral {
    color: ${(props) => props.theme.css.toneNeutral};
  }
  .tone-icon {
    height: 20px;
    width: 20px;
    margin-right: 0.5rem;
    flex-shrink: 0;
  }
  .checkbox {
    margin-right: 0.5rem;
  }
  .date {
    font-family: ${(props) => props.theme.css.fPrimary};
    white-space: nowrap;
    display: flex;
  }
  .section,
  .page-number {
    font-family: ${(props) => props.theme.css.fPrimary};
  }

  .page-number {
    margin-left: 0.25rem;
  }

  .new-tab {
    margin-right: 0.5rem;
    margin-left: 0.5rem;
  }

  .play-icon,
  .eye-slash,
  .new-tab {
    height: 20px;
    width: 20px;
    color: ${(props) => props.theme.css.btnBkPrimary};
    &:hover {
      cursor: pointer;
      transform: scale(1.1);
    }
  }
  .teaser {
    &.with-grip {
      margin-left: 5.75rem;
    }
    color: ${(props) => props.theme.css.btnSecondaryColor};
    margin-left: 4rem;
    background-color: ${(props) => props.theme.css.teaserBackground};
    font-family: ${(props) => props.theme.css.fPrimary};
    font-size: 1rem;
    font-weight: 400;
    border-radius: 0.25rem;
    padding: 0.2rem;
    margin-bottom: 0.25rem;
    margin-right: 7.25rem;
    cursor: default;
  }
  .headline {
    font-family: ${(props) => props.theme.css.fPrimary};
    font-size: 1.1rem;
    text-decoration: none;
    margin-right: auto;
    &:hover {
      cursor: pointer;
    }
  }
  .checked {
    background-color: ${(props) => props.theme.css.highlightYellow};
    .teaser {
      background-color: ${(props) => props.theme.css.highlightYellowDarker};
    }
  }
  .grip {
    height: 20px;
    margin-right: 0.5rem;
    color: ${(props) => props.theme.css.btnBkPrimary};
    cursor: row-resize;
  }
  .content-report-pin {
    margin-left: 0.5rem;
  }
`;
