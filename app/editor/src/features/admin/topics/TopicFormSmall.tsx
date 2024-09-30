import { FormikForm } from 'components/formik';
import React from 'react';
import { useApp } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  FieldSize,
  FormikText,
  ITopicModel,
  Loader,
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
  const [{ requests }] = useApp();

  const handleSubmit = async (values: ITopicModel) => {
    try {
      onAddOrUpdate?.(values);
    } catch {}
  };

  return (
    <styled.TopicFormSmall>
      <Loader visible={requests.some((r) => ['update-topic', 'add-topic'].includes(r.url))} />
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
              <FormikText name="name" label="Topic Name" width={FieldSize.Big} autoComplete="off" />
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
