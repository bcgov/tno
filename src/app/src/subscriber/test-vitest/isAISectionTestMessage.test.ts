import { describe, expect, it } from 'vitest';

import {
  getAISectionTestId,
  isAISectionTestMessage,
} from '../features/my-reports/utils/isAISectionTestMessage';

describe('isAISectionTestMessage', () => {
  it('recognizes only the test run prefix', () => {
    expect(isAISectionTestMessage({ message: 'ai-test:abc' })).toBe(true);
    expect(getAISectionTestId({ message: 'ai-test:abc' })).toBe('abc');
    expect(isAISectionTestMessage({ message: 'ai-sections' })).toBe(false);
    expect(isAISectionTestMessage({ message: 'status' })).toBe(false);
    expect(isAISectionTestMessage({ message: undefined })).toBe(false);
    expect(isAISectionTestMessage(undefined)).toBe(false);
    expect(getAISectionTestId({ message: 'status' })).toBeUndefined();
  });
});
