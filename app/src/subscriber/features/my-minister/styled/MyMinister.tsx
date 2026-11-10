import styled from 'styled-components';

export const MyMinister = styled.div`
  .content-list {
    width: 100%;
  }

  .td-date {
    white-space: nowrap;
  }

  .ministerCheckboxes {
    display: flex;
  }

  .option {
    margin: 5px;
    font-weight: bold;
  }

  .mentions {
    display: flex;
    max-height: fit-content;
  }

  .mention-tag {
    display: flex;
    border-radius: 4px;
    background: ${(props) => props.theme.css.tagBackgroundColor};
    padding: 4px 4px;
    justify-content: center;
    gap: 2px;
    color: #000;
    text-align: center;
    font-family: ${(props) => props.theme.css.fPrimary};
    font-size: 12px;
    font-style: normal;
    font-weight: 600;
    line-height: 12px; /* 100% */
    text-transform: uppercase;
  }

  .check-label {
    margin-top: 0.35em;
    margin-right: 1em;
  }
`;
