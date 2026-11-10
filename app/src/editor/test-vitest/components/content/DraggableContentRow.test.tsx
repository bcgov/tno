import { render, screen } from '@testing-library/react';
import { DraggableContentRow } from 'components/content/DraggableContentRow';
import React from 'react';
import { DragDropContext, Droppable } from 'react-beautiful-dnd';
import { TestWrapper } from 'test/utils';
import { ContentStatusName, ContentTypeName, UserAccountTypeName, UserStatusName } from 'tno-core';

describe('DraggableContentRow', () => {
  const mockRow = {
    content: {
      id: 1,
      headline: 'Test Headline',
      owner: {
        id: 1,
        key: 'test-key',
        username: 'testuser',
        email: 'test@example.com',
        preferredEmail: '',
        displayName: 'Test User',
        firstName: 'Test',
        lastName: 'User',
        isEnabled: true,
        status: UserStatusName.Activated,
        emailVerified: true,
        isSystemAccount: false,
        accountType: UserAccountTypeName.Direct,
        uniqueLogins: 0,
        note: '',
        roles: [],
        createdBy: 'system',
        createdOn: new Date().toISOString(),
        updatedBy: 'system',
        updatedOn: new Date().toISOString(),
        version: 0,
      },
      status: ContentStatusName.Published,
      contentType: ContentTypeName.AudioVideo,
      licenseId: 1,
      otherSource: '',
      mediaTypeId: 1,
      byline: '',
      edition: '',
      section: '',
      page: '',
      publishedOn: new Date().toISOString(),
      summary: '',
      body: '',
      isHidden: false,
      isApproved: false,
      isPrivate: false,
      actions: [],
      tags: [],
      labels: [],
      topics: [],
      tonePools: [],
      timeTrackings: [],
      fileReferences: [],
      links: [],
      quotes: [],
      userNotifications: [],
      versions: {},
      createdBy: 'test',
      createdOn: new Date().toISOString(),
      updatedBy: 'test',
      updatedOn: new Date().toISOString(),
      version: 0,
    },
    sortOrder: 0,
    selected: false,
  };

  const renderComponent = (props = {}) => {
    return render(
      <TestWrapper>
        <DragDropContext onDragEnd={() => {}}>
          <Droppable droppableId="test">
            {(provided) => (
              <div {...provided.droppableProps} ref={provided.innerRef}>
                <DraggableContentRow
                  draggableId="test-draggable"
                  index={0}
                  row={mockRow}
                  {...props}
                />
                {provided.placeholder}
              </div>
            )}
          </Droppable>
        </DragDropContext>
      </TestWrapper>,
    );
  };

  it('renders without crashing', () => {
    renderComponent();
    expect(screen.getByText('Test Headline')).toBeInTheDocument();
  });

  it('renders custom children', () => {
    renderComponent({
      children: () => <div data-testid="custom-child">Custom Content</div>,
    });

    expect(screen.getByTestId('custom-child')).toBeInTheDocument();
    expect(screen.getByText('Custom Content')).toBeInTheDocument();
  });
});
