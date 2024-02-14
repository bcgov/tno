import { FormikForm } from 'components/formik';
import React from 'react';
import { toast } from 'react-toastify';
import {
  Button,
  ButtonVariant,
  FieldSize,
  FormikText,
  ITopicModel,
  Row,
  ToggleGroup,
  TopicTypeName,
} from 'tno-core';

import { defaultTopic } from './constants';
import * as styled from './styled';

interface ITopicFormSmallProps {
  onAddOrUpdate: (value: ITopicModel) => void;
}

/** The page used to view and edit tags in the administrative section. */
export const TopicFormSmall: React.FC<ITopicFormSmallProps> = ({ onAddOrUpdate }) => {
  const handleSubmit = async (values: ITopicModel) => {
    try {
      onAddOrUpdate?.(values);
    } catch {
      toast.error(`Error saving topic ${values.name}.`);
    }
  };

  return (
    <styled.TopicFormSmall>
      <FormikForm
        loading={false}
        initialValues={defaultTopic}
        onSubmit={async (values, { setSubmitting }) => {
          await handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <>
            <Row>
              <p>
                <b>Create a new Topic</b>
              </p>
            </Row>
            <Row alignItems="center">
              <FormikText name="name" label="Topic Name" width={FieldSize.Big} />
              <div className="topic-type-toggle-group frm-in">
                <label>Type</label>
                <ToggleGroup
                  defaultSelected={TopicTypeName.Issues}
                  options={Object.values(TopicTypeName).map((i) => ({
                    id: i,
                    label: i,
                    data: i,
                    onClick: () => {
                      setFieldValue('topicType', i);
                    },
                  }))}
                ></ToggleGroup>
              </div>
              <Button
                type="submit"
                variant={ButtonVariant.primary}
                disabled={isSubmitting || !values.name || !values.topicType}
                style={{ marginTop: '15px' }}
              >
                Save
              </Button>
            </Row>
          </>
        )}
      </FormikForm>
    </styled.TopicFormSmall>
  );
};
