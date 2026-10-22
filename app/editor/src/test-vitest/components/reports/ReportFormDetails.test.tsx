import { act, fireEvent, render, screen } from '@testing-library/react';
import { ReportFormDetails } from 'features/admin/reports/ReportFormDetails';
import { Formik, useFormikContext } from 'formik';
import React from 'react';
import { TestWrapper } from 'test/utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

// Stable identities, the way the Redux-backed hooks behave.
const app = vi.hoisted(() => ({ state: { userInfo: { id: 1, roles: [] as string[] } } }));
const users = vi.hoisted(() => ({ findUsers: vi.fn() }));

vi.mock('store/hooks', () => ({ useApp: () => [app.state] }));
vi.mock('store/hooks/admin', () => ({ useUsers: () => [{}, users] }));

const owner = { id: 900, username: 'jdoe', displayName: 'Jane Doe', isEnabled: true };
const other = { id: 901, username: 'bsmith', displayName: 'Bob Smith', isEnabled: true };

const report = {
  id: 5,
  name: 'Daily',
  description: '',
  ownerId: owner.id,
  owner,
  sortOrder: 0,
  isEnabled: true,
  isPublic: false,
  sections: [],
  settings: { subject: { text: 'Daily' } },
} as any;

let values: any;
const Values = () => {
  values = useFormikContext().values;
  return null;
};

const renderDetails = () =>
  render(
    <TestWrapper>
      <Formik initialValues={report} onSubmit={() => {}}>
        <>
          <ReportFormDetails />
          <Values />
        </>
      </Formik>
    </TestWrapper>,
  );

describe('ReportFormDetails owner', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    users.findUsers.mockReset().mockResolvedValue({ items: [other], total: 1 });
  });
  afterEach(() => vi.useRealTimers());

  it('displays the report owner without loading the user list', () => {
    renderDetails();
    expect(screen.getByText('Jane Doe [jdoe]')).toBeInTheDocument();
    expect(users.findUsers).not.toHaveBeenCalled();
  });

  it('searches users as you type and changes the owner', async () => {
    renderDetails();
    const input = screen.getByRole('combobox');
    await act(async () => {
      fireEvent.change(input, { target: { value: 'bob' } });
      fireEvent.change(input, { target: { value: 'bobs' } });
    });
    await act(async () => await vi.advanceTimersByTimeAsync(600));

    expect(users.findUsers).toHaveBeenCalledTimes(1);
    expect(users.findUsers.mock.calls[0][0]).toMatchObject({ keyword: 'bobs', quantity: 50 });
    expect(users.findUsers.mock.calls[0][1]).toBe(true);

    await act(async () => {
      fireEvent.click(screen.getByText('Bob Smith [bsmith]'));
    });
    expect(values.ownerId).toBe(other.id);
    expect(values.owner).toEqual(other);
    expect(values.values).toBeUndefined();
    expect(screen.getByText('Bob Smith [bsmith]')).toBeInTheDocument();
  });
});
