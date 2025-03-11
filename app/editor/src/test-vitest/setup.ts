import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// clean up after each test 
afterEach(() => {
  cleanup();
}); 