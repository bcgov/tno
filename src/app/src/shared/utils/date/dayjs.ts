import dayjs, { type Dayjs, type PluginFunc } from 'dayjs';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import isSameOrAfter from 'dayjs/plugin/isSameOrAfter';
import isSameOrBefore from 'dayjs/plugin/isSameOrBefore';
import minMax from 'dayjs/plugin/minMax';
import timezone from 'dayjs/plugin/timezone';
import utc from 'dayjs/plugin/utc';
import weekday from 'dayjs/plugin/weekday';

/**
 * Keeps the string output the apps had with moment, which values are displayed with and sent to
 * the API with:
 * - toString() (and toLocaleString(), which calls it): moment's 'Tue Nov 10 2026 14:00:00 GMT-0800'
 *   rather than Day.js's toUTCString().
 * - An invalid date formats as 'Invalid date' (Day.js: 'Invalid Date').
 * - An invalid date's toISOString() is null (Day.js throws a RangeError).
 */
const momentCompatible: PluginFunc = (_option, dayjsClass) => {
  const proto = dayjsClass.prototype;
  const format = proto.format;
  const toISOString = proto.toISOString;
  proto.format = function (this: Dayjs, template?: string) {
    return this.isValid() ? format.call(this, template) : 'Invalid date';
  };
  proto.toString = function (this: Dayjs) {
    return this.isValid() ? format.call(this, 'ddd MMM DD YYYY HH:mm:ss [GMT]ZZ') : 'Invalid date';
  };
  proto.toISOString = function (this: Dayjs) {
    return (this.isValid() ? toISOString.call(this) : null) as string;
  };
};

dayjs.extend(customParseFormat);
dayjs.extend(isSameOrAfter);
dayjs.extend(isSameOrBefore);
dayjs.extend(minMax);
dayjs.extend(utc);
dayjs.extend(timezone);
dayjs.extend(weekday);
// Last, so it wraps the formatting added by the plugins above.
dayjs.extend(momentCompatible);

export { dayjs };
export type { Dayjs };
