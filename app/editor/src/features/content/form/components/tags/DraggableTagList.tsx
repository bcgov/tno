import React from 'react';
import Draggable from 'react-draggable';
import { useLookup } from 'store/hooks';
import { Col, GridTable, Row, Show } from 'tno-core';

import { columns } from './constants/columns';

export interface IDraggableTagListProps {
  showList: boolean;
  setShowList: (showList: boolean) => void;
}

/**
 * A draggable list of tags for the user to reference
 * @returns A Tags list component
 */
export const DraggableTagList: React.FC<IDraggableTagListProps> = ({ showList, setShowList }) => {
  const [{ tags }] = useLookup();
  const nodeRef = React.useRef(null);

  return (
    <Draggable nodeRef={nodeRef}>
      <div ref={nodeRef}>
        <Show visible={showList}>
          <Col className="tag-list" id="tag-list">
            <Row className="tag-list-header">
              <h2>Available Tags</h2>
              <h2 className="close" onClick={() => setShowList(false)}>
                X
              </h2>
            </Row>
            <GridTable data={tags} columns={columns} />
          </Col>
        </Show>
      </div>
    </Draggable>
  );
};
