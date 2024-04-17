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
  .react-datepicker-ignore-onclickoutside {
    border-radius: 0.5rem;
    height: 1.5em;
  }
  .react-datepicker__input-container {
    input {
      height: 1.6rem;
      border-radius: 0.25em;
    }
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
  .react-datepicker__time-list-item--selected,
  .react-datepicker__close-icon::after {
    background-color: ${(props) => props.theme.css.btnRedColor};
  }
  .react-datepicker__close-icon::after {
    background-color: ${(props) => props.theme.css.btnBkPrimary};
  }
  .react-datepicker__day--keyboard-selected,
  .react-datepicker__day--in-selecting-range {
    background-color: ${(props) => props.theme.css.btnLightRedColor};
  }

  .react-datepicker__day--in-range,
  .react-datepicker__day--selected {
    background-color: ${(props) => props.theme.css.btnRedColor};
  }

  .react-datepicker__day--keyboard-selected,
  .react-datepicker__month-text--keyboard-selected,
  .react-datepicker__quarter-text--keyboard-selected,
  .react-datepicker__year-text--keyboard-selected {
    &:hover {
      background-color: ${(props) => props.theme.css.btnRedColor};
    }
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
    justify-content: flex-end;
    gap: 1rem;
  }
  .remove-container {
    border-top: 0.1rem solid ${(props) => props.theme.css.btnBkPrimary};
    margin-top: 1rem;
    padding-top: 0.5rem;
    .remove-action-buttons {
      > div {
        justify-content: center;

        .danger {
          svg {
            color: ${(props) => props.theme.css.fRedColor};
          }
        }
      }
      .warning {
        margin-right: 0.5rem;
      }
    }
  }
`;
