/**
 * Temporary fix that stops react-tooltip infinite loop error.
 * This bug occurs when clicking the Add to Folder/Report if there is content on the page.
 * "ResizeObserver loop completed with undelivered notifications"
 */
export const fixTooltipResizeObserver = () => {
  // Stop error resizeObserver
  const debounce = (callback: (...args: any[]) => void, delay: number) => {
    let tid: any;
    return function (...args: any[]) {
      // eslint-disable-next-line no-restricted-globals
      const ctx = self;
      tid && clearTimeout(tid);
      tid = setTimeout(() => {
        callback.apply(ctx, args);
      }, delay);
    };
  };

  const _ = (window as any).ResizeObserver;
  (window as any).ResizeObserver = class ResizeObserver extends _ {
    constructor(callback: (...args: any[]) => void) {
      callback = debounce(callback, 20);
      super(callback);
    }
  };
};
