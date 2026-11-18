import { describe, expect, it } from 'vitest';

import { dayjs } from './dayjs';

// Expected values were produced by moment (which the apps used before Day.js) for the same input.
const instant = '2026-03-08T10:05:09.123Z';

describe('shared dayjs', () => {
  it('formats like moment did', () => {
    const utc: Record<string, string> = {
      'YYYY-MM-DD': '2026-03-08',
      'MMM D, YYYY HH:mm:ss': 'Mar 8, 2026 10:05:09',
      'HH:mm:ss': '10:05:09',
      'YYYY/MM/DD': '2026/03/08',
      'YYYY-MM-DD hh:mm:ssA': '2026-03-08 10:05:09AM',
      'DD-MMM-YYYY': '08-Mar-2026',
      'YYYY-MM-DD HH:mm:ss a': '2026-03-08 10:05:09 am',
      'YYYY-MM-DDTHH:mm:ss': '2026-03-08T10:05:09',
      'MMMM DD, YYYY HH:mm:ss': 'March 08, 2026 10:05:09',
      'MM/DD/YYYY HH:mm:ss': '03/08/2026 10:05:09',
      YYYYMMDD: '20260308',
      'MMMM DD - YYYY': 'March 08 - 2026',
      'dddd, MMMM DD, YYYY': 'Sunday, March 08, 2026',
      'MMM-DD': 'Mar-08',
      'MMM-YYYY': 'Mar-2026',
      'MM/DD HH:mm:ss A': '03/08 10:05:09 AM',
      'DD-MM-YYYY hh:mm:ss': '08-03-2026 10:05:09',
    };
    const vancouver: Record<string, string> = {
      'YYYY-MM-DD': '2026-03-08',
      'MMM D, YYYY HH:mm:ss': 'Mar 8, 2026 03:05:09',
      'HH:mm:ss': '03:05:09',
      'YYYY/MM/DD': '2026/03/08',
      'YYYY-MM-DD hh:mm:ssA': '2026-03-08 03:05:09AM',
      'DD-MMM-YYYY': '08-Mar-2026',
      'YYYY-MM-DD HH:mm:ss a': '2026-03-08 03:05:09 am',
      'YYYY-MM-DDTHH:mm:ss': '2026-03-08T03:05:09',
      'MMMM DD, YYYY HH:mm:ss': 'March 08, 2026 03:05:09',
      'MM/DD/YYYY HH:mm:ss': '03/08/2026 03:05:09',
      YYYYMMDD: '20260308',
      'MMMM DD - YYYY': 'March 08 - 2026',
      'dddd, MMMM DD, YYYY': 'Sunday, March 08, 2026',
      'MMM-DD': 'Mar-08',
      'MMM-YYYY': 'Mar-2026',
      'MM/DD HH:mm:ss A': '03/08 03:05:09 AM',
      'DD-MM-YYYY hh:mm:ss': '08-03-2026 03:05:09',
    };
    for (const [format, expected] of Object.entries(utc))
      expect(dayjs.utc(instant).format(format), format).toBe(expected);
    for (const [format, expected] of Object.entries(vancouver))
      expect(dayjs(instant).tz('America/Vancouver').format(format), format).toBe(expected);
  });

  it("keeps moment's toString()", () => {
    expect(dayjs.utc(instant).toString()).toBe('Sun Mar 08 2026 10:05:09 GMT+0000');
    expect(dayjs(instant).tz('America/Vancouver').toString()).toBe(
      'Sun Mar 08 2026 03:05:09 GMT-0700',
    );
    expect(`${dayjs.utc(instant)}`).toBe('Sun Mar 08 2026 10:05:09 GMT+0000');
  });

  it('treats invalid dates like moment did', () => {
    const invalid = dayjs('not a date');
    expect(invalid.isValid()).toBe(false);
    expect(invalid.format('YYYY-MM-DD')).toBe('Invalid date');
    expect(invalid.toString()).toBe('Invalid date');
    expect(invalid.toISOString()).toBeNull();
  });

  it('parses strictly, converts zones and compares', () => {
    expect(dayjs('2026-11-10', 'YYYY-MM-DD', true).isValid()).toBe(true);
    expect(dayjs('2026-13-10', 'YYYY-MM-DD', true).isValid()).toBe(false);
    expect(dayjs.utc(instant).tz('UTC', true).toISOString()).toBe('2026-03-08T10:05:09.123Z');
    expect(dayjs.min(dayjs.utc(instant), dayjs.utc('2026-01-01'))!.toISOString()).toBe(
      '2026-01-01T00:00:00.000Z',
    );
    expect(dayjs.utc(instant).isSameOrAfter(dayjs.utc('2026-01-01'))).toBe(true);
    expect(dayjs.utc(instant).add(-1, 'day').startOf('day').toISOString()).toBe(
      '2026-03-07T00:00:00.000Z',
    );
  });
});
