import { ReactInstance } from 'react';

// @types/react 18.3 added `refs` as required on Component, but older class-based
// third-party packages (react-datepicker, react-text-mask) predate this and don't
// declare it explicitly. These augmentations restore compatibility.

declare module 'react-datepicker' {
  interface ReactDatePickerProps {
    refs?: { [key: string]: ReactInstance };
  }
  class ReactDatePicker {
    refs: { [key: string]: ReactInstance };
  }
}

declare module 'react-text-mask' {
  class MaskedInput {
    refs: { [key: string]: ReactInstance };
  }
}
