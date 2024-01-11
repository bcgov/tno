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

  .list-title {
    padding-left: 7px;
    font-size: 20px;
    font-weight: bold;
  }

  .row {
    & .column {
      align-items: center;
      & .frm-in {
        padding: unset;
      }
    }
  }
  .type-none {
    color: red;
  }
  .type-Proactive {
    color: green;
  }
  .type-disabled {
    font-style: italic;
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
`;
