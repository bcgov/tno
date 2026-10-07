import React from 'react';
import { Button, ButtonVariant, Col, Modal } from 'tno-core';

import { type ITopicScoreSourceModel } from './interfaces';

interface IRemoveScoringSourceProps {
  source: ITopicScoreSourceModel;
  onRemove: () => Promise<void>;
  onClose: () => void;
}

/** Explain what removing a source from topic scoring changes before applying it. */
export const RemoveScoringSource: React.FC<IRemoveScoringSourceProps> = ({
  source,
  onRemove,
  onClose,
}) => {
  const [saving, setSaving] = React.useState(false);
  const handleRemove = async () => {
    setSaving(true);
    try {
      await onRemove();
    } catch {
      // Errors are handled globally; keep the dialog open for retry.
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      isShowing
      type="custom"
      headerText="Remove source from topic scoring"
      component={
        <Col>
          <p>
            Remove <strong>{source.name}</strong> from topic scoring?
          </p>
          <p>
            This turns off Use in Topics for the source and its series. The source, its rules,
            default score, and existing story scores are kept. You can add the source again later.
          </p>
        </Col>
      }
      customButtons={
        <>
          <Button variant={ButtonVariant.danger} onClick={handleRemove} disabled={saving}>
            Remove source
          </Button>
          <Button variant={ButtonVariant.secondary} onClick={onClose} disabled={saving}>
            Cancel
          </Button>
        </>
      }
    />
  );
};
