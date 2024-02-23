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
  .type-not-applicable {
    // color: red;
    font-style: italic;
  }
  .type-Proactive {
    color: #006600;
  }
  .type-Issues {
    color: #bb1111;
  }
  .type-disabled {
    font-style: italic;
  }
  .option-hint {
    display: inline-block;
    color: rgb(255, 255, 255);
    border-radius: 50%;
    font-size: 0.75rem;
    width: 1rem;
    height: 1rem;
    text-align: center;
    font-weight: bold;
    margin-right: 0.25rem;
    text-transform: uppercase;
  }
  .option-hint.type-Proactive {
    background-color: #006600;
  }
  .option-hint.type-Issues {
    background-color: #bb1111;
  }
  .option-hint.type-not-applicable {
    display: none;
  }
  .topic-select {
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
  .col-3 .frm-in {
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
`;
