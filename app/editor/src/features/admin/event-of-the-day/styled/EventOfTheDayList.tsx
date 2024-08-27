import styled from 'styled-components';

export const EventOfTheDayList = styled.div`
  display: flex;
  flex-flow: column;
  flex-grow: 1;
  .form-page {
    display: flex;
    flex-flow: column;
  }

  .row-header {
    font-weight: bold;
    padding: 0.25em 0.5em 0.25em 0.5em;
    border-top: solid 2px black;
    border-bottom: solid 2px black;
  }

  .row {
    padding: 0.5em 0.5em 0 0.5em;
  }

  .page-header {
    margin-bottom: 1rem;
    .list-title {
      padding-left: 7px;
      font-size: 20px;
      font-weight: bold;
    }
    .buttons {
      margin-left: auto;
      display: flex;
      gap: 0.5rem;
      .icon {
        align-self: center;
        margin-left: 0.5rem;
      }
    }
  }

  .row {
    & .column {
      align-items: center;
      & .frm-in {
        padding: unset;
      }
    }
  }
  div.col-3 > div {
    width: 100%;
  }
  .score-select {
    width: 10ch;
  }
  .score-max-hint-text {
    font-style: unset;
    color: green;
    border-bottom: 1px green dashed;
    cursor: help;
  }
  .score-max-no-rule-match {
    font-style: unset;
    color: red;
    border-bottom: 1px red dashed;
    cursor: help;
  }
  .column.col-0 {
    gap: unset;
    display: block;
    align-self: center;
  }
  .column.col-3 .frm-in {
    width: 100%;
  }
  .lock-control {
    pointer-events: none;
  }
  .row:has(div a.lock-control) {
    cursor: wait;
  }
  .topic-form-row {
    .form-page {
      padding: unset;
    }
  }
  .clipboard-icon {
    visibility: hidden;
    cursor: pointer;
    margin-left: 0.25rem;
  }
  .row:hover .clipboard-icon {
    visibility: visible;
  }
  .clipboard-icon.animate {
    transition: all 1s;
    transform: rotateY(180deg);
  }

  .icon-refresh {
    color: #04814d !important;
    transition: color 0.3s ease;
  }
  .icon-refresh:hover {
    transform: rotate(-90deg);
  }
  .icon-refresh:active svg {
    color: #26e194 !important;
  }
`;
