import { PageSection } from 'components/section';
import styled from 'styled-components';

export const ConfigureFolder = styled(PageSection)`
  .gear {
    color: ${(props) => props.theme.css.btnBkPrimary};
    margin-right: 1rem;
  }
  .small-gear {
    color: ${(props) => props.theme.css.iconGrayColor};
    align-self: center;
  }
  .time-inputs {
    margin-top: 0.5rem;
    margin-bottom: 0.5rem;
    input {
      height: 2.5rem;
      box-sizing: border-box;
      border-radius: 0.5rem;
      border-thickness: 0.1rem;
      border-color: black;
      box-shadow: none;
    }
  }
  .react-datepicker-ignore-onclickoutside {
    border-radius: 0.5rem;
  }
  button:not(.react-datepicker__close-icon):not(.warning):not(.danger):not(.cancel) {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
    border: none;
    border-radius: 0.5rem;
    font-size: 0.85rem;
    max-width: fit-content;
    height: 2.5rem;
  }
  button {
    border-radius: 0.5rem;
    font-size: 0.85rem;
    max-width: fit-content;
    height: 2.5rem;
  }

  .danger {
    &:hover {
      color: ${(props) => props.theme.css.btnRedColor};
      filter: grayscale(0.2);
    }
  }

  .warning {
    &:hover {
      color: ${(props) => props.theme.css.btnYellowColor};
      filter: grayscale(0.2);
    }
  }
  .react-datepicker__close-icon::after {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .main-container {
    padding: 1rem;
    .schedule-content {
      width: 100%;
    }
  }
  h3 {
    margin-bottom: 0;
    margin-left: 0.5rem;
  }
  label {
    font-weight: 900;
  }

  .add-filter,
  .add-schedule {
    margin-left: 1.75rem;
  }

  .add-filter {
    margin-bottom: 1rem;
    .frm-in {
      width: 70%;
    }
  }

  .main-sched-body {
    margin-left: 2.05rem;
  }

  .keep-stories {
    align-items: center;
    input {
      margin-left: 0.5rem;
      width: 4rem;
    }
    svg {
      color: ${(props) => props.theme.css.btnBkPrimary};
      margin-left: 0.5rem;
    }
  }
  .action-buttons {
    width: 100%;
    align-items: end;
    .cancel {
      color: ${(props) => props.theme.css.btnBkPrimary};
      background-color: ${(props) => props.theme.css.bkWhite};
      border: 0.1rem solid ${(props) => props.theme.css.btnBkPrimary};
      margin-right: 0.5rem;
      margin-left: auto;
    }
  }
  .remove-container {
    border-top: 0.1rem solid ${(props) => props.theme.css.btnBkPrimary};
    margin-top: 1rem;
    padding-top: 0.5rem;
    .remove-action-buttons {
      margin-left: 1.5rem;
      .warning {
        margin-right: 0.5rem;
      }
    }
  }
`;
