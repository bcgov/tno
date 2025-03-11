# Test Documentation (Example for app/editor)

## Test Directory Structure 

```
src/test-vitest/
├── components/          # Component tests
│   └── content/        # Content-related component tests
├── data/               # Test data
├── utils/              # Test utilities
│   ├── TestWrapper.tsx # Test wrapper component
│   └── index.ts       # Utility exports
└── setup.ts           # Global test setup
```

## Test Environment Setup

### Global Setup (setup.ts)

```typescript
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(() => {
  cleanup();
});
```

This setup file:
- Imports jest-dom extended assertions
- Automatically cleans up the DOM after each test

### Test Wrapper (TestWrapper.tsx)

We provide a unified test wrapper that includes the following features:

1. Redux Store Configuration
   - todo need mock store
   - Serialization checks are disabled for testing convenience

2. Router Support
   - Uses BrowserRouter to provide routing environment

3. Theme Support
   - Integrates the project's theme variables
   - Provides complete style context

Usage example:

```typescript
import { render } from '@testing-library/react';
import { TestWrapper } from '../utils/TestWrapper';

describe('YourComponent', () => {
  it('should render correctly', () => {
    render(
      <TestWrapper>
        <YourComponent />
      </TestWrapper>
    );
  });
});
```

## Test Writing Guidelines

### Component Test Specifications

1. File Location
   - Component test files should be placed in the `components` directory
   - Maintain the same directory structure as the source code

2. File Naming
   - Use `.test.tsx` as the test file extension
   - Test file names should correspond to component file names

3. Test Structure
```typescript
import { render, screen } from '@testing-library/react';
import { TestWrapper } from '../../utils/TestWrapper';
import YourComponent from './YourComponent';

describe('YourComponent', () => {
  // Preparation work before each test
  beforeEach(() => {
    // Initialization work
  });

  // Basic rendering test
  it('should render the component correctly', () => {
    render(
      <TestWrapper>
        <YourComponent />
      </TestWrapper>
    );
    // Verify rendering results
  });

  // Interaction test
  it('should respond correctly to user interactions', async () => {
    render(
      <TestWrapper>
        <YourComponent />
      </TestWrapper>
    );
    // Simulate user interactions
    // Verify interaction results
  });
});
```

### Test Data Management

1. Test data should be placed in the `data` directory
2. Organize test data files by functional modules
3. Use TypeScript types to ensure correct data structures

## Test Command Usage Guide

We use Vitest as our testing framework. Here are commonly used test commands:

```bash
# Run all tests (single run)
yarn test

# Run tests in watch mode (recommended during development)
yarn test:watch

# Run tests with UI interface (convenient for debugging)
yarn test:ui

# Run specific test file
yarn test path/to/your/test.test.tsx

# Run tests matching specific file pattern
yarn test "components/**/*.test.tsx"
```


### Configuration Details

Test-related configuration in `package.json`:

```json
{
  "scripts": {
    "test": "vitest run",      // Single run of all tests
    "test:watch": "vitest",    // Run tests in watch mode
    "test:ui": "vitest --ui"   // Run tests with UI interface
  }
}
```

## Test Development Workflow

1. During development, it's recommended to use `yarn test:watch`
   - Real-time monitoring of file changes
   - Immediate test feedback
   - Support for test filtering

2. Use `yarn test:ui` when debugging complex test cases
   - Provides visual interface
   - More intuitive test result display
   - Convenient debugging tools

3. Use `yarn test` in CI/CD environments
   - Single run of all tests
   - Suitable for automated processes
   
## Common Issues and Solutions

1. Component Rendering Issues
   - Ensure TestWrapper is being used
   - Check if Redux store state needs to be mocked

2. Asynchronous Testing Issues
   - Use `async/await` to handle asynchronous operations
   - Use `waitFor` to wait for state updates

3. Event Triggering Issues
   - Use `fireEvent` or `userEvent` to simulate user operations
   - Ensure event target elements can be correctly selected