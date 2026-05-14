import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useLLMs } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikText,
  FormikTextArea,
  FormikWysiwyg,
  IconButton,
  type ILLMModel,
  LabelPosition,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';
import { object, string } from 'yup';

import { defaultLLM } from './constants';
import * as styled from './styled';

const LLMSchema = object({
  name: string().required('Name is required'),
  deploymentName: string().required('Deployment Name is required'),
  systemPrompt: string().required('Default System Prompt is required'),
});

const toOptionalString = (val?: string) =>
  !val || val.trim() === '' || val === '<p><br></p>' ? undefined : val;

const LLMForm: React.FC = () => {
  const [, api] = useLLMs();
  const { state } = useLocation();
  const { id } = useParams();
  const navigate = useNavigate();

  const [llm, setLLM] = React.useState<ILLMModel>(state?.llm ?? defaultLLM);

  const llmId = Number(id);
  const { toggle, isShowing } = useModal();

  React.useEffect(() => {
    if (!!llmId && llm?.id !== llmId) {
      setLLM({ ...defaultLLM, id: llmId });
      api.getLLM(llmId).then((data) => {
        setLLM(data);
      });
    }
  }, [api, llm?.id, llmId]);

  const handleSubmit = async (values: ILLMModel) => {
    try {
      const originalId = values.id;
      const payload: ILLMModel = {
        ...values,
        apiKey: toOptionalString(values.apiKey),
        projectEndpoint: toOptionalString(values.projectEndpoint),
      };
      const result = !llm.id ? await api.addLLM(payload) : await api.updateLLM(payload);
      setLLM(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/llms/${result.id}`);
    } catch {}
  };

  return (
    <styled.LLMForm>
      <IconButton
        iconType="back"
        label="Back to LLMs"
        className="back-button"
        onClick={() => {
          navigate('/admin/llms');
        }}
      />
      <FormikForm
        initialValues={llm}
        validationSchema={LLMSchema}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
        validateOnBlur={true}
        validateOnChange={false}
        validateOnMount={false}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <div className="form-container">
            <Col className="form-inputs">
              <Row gap="1rem">
                <FormikText width={FieldSize.Large} name="name" label="Name" />
                <FormikCheckbox
                  labelPosition={LabelPosition.Top}
                  label="Is Public"
                  name="isPublic"
                  tooltip="Allow non-admin users to select this model"
                />
                <FormikCheckbox
                  labelPosition={LabelPosition.Top}
                  label="Is Enabled"
                  name="isEnabled"
                />
              </Row>
              <FormikTextArea name="description" label="Description" width={FieldSize.Large} />
              <Row gap="1rem">
                <FormikText
                  width={FieldSize.Large}
                  name="deploymentName"
                  label="Deployment Name"
                  tooltip="The model deployment name used when calling the AI API"
                />
                <Row gap="1rem">
                  <FormikText
                    width={FieldSize.Tiny}
                    name="minTemperature"
                    label="Min Temperature"
                    type="number"
                    onChange={(e) =>
                      setFieldValue(
                        'minTemperature',
                        e.target.value === '' ? undefined : parseFloat(e.target.value),
                      )
                    }
                  />
                  <FormikText
                    width={FieldSize.Tiny}
                    name="maxTemperature"
                    label="Max Temperature"
                    type="number"
                    onChange={(e) =>
                      setFieldValue(
                        'maxTemperature',
                        e.target.value === '' ? undefined : parseFloat(e.target.value),
                      )
                    }
                  />
                </Row>
              </Row>
              <FormikText width={FieldSize.Large} name="agentName" label="Agent Name" />
              <FormikText
                width={FieldSize.Large}
                name="projectEndpoint"
                label="Project Endpoint"
                tooltip="URL to the AI project API endpoint"
                onChange={(e) =>
                  setFieldValue(
                    'projectEndpoint',
                    e.target.value === '' ? undefined : e.target.value,
                  )
                }
              />
              <FormikText
                width={FieldSize.Large}
                name="apiKey"
                label="API Key"
                type="password"
                tooltip="API key for authenticating with the AI service"
                onChange={(e) =>
                  setFieldValue('apiKey', e.target.value === '' ? undefined : e.target.value)
                }
              />
              <FormikWysiwyg
                name={'systemPrompt'}
                label="Default System Prompt:"
                placeholder="Enter a default system prompt"
              />
              <FormikWysiwyg
                name={'userPrompt'}
                label="Default User Prompt:"
                placeholder="Enter a default user prompt"
              />
              <FormikText
                width={FieldSize.Tiny}
                name="sortOrder"
                label="Sort Order"
                type="number"
                className="sort-order"
              />
              <Show visible={!!values.id}>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="updatedBy"
                    label="Updated By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      values.updatedOn ? moment(values.updatedOn).toString() : undefined
                    }
                    onChange={noop}
                    name="updatedOn"
                    label="Updated On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
                <Row>
                  <FormikText
                    width={FieldSize.Small}
                    disabled
                    name="createdBy"
                    label="Created By"
                  />
                  <FormikDatePicker
                    selectedDate={
                      values.createdOn ? moment(values.createdOn).toString() : undefined
                    }
                    onChange={noop}
                    name="createdOn"
                    label="Created On"
                    disabled
                    width={FieldSize.Small}
                  />
                </Row>
              </Show>
            </Col>
            <Row justifyContent="center" className="form-inputs">
              <Button type="submit" disabled={isSubmitting}>
                Save
              </Button>
              <Show visible={!!values.id}>
                <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                  Delete
                </Button>
              </Show>
            </Row>
            <Modal
              headerText="Confirm Removal"
              body="Are you sure you wish to remove this LLM?"
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                try {
                  await api.deleteLLM(llm);
                  toast.success(`${llm.name} has successfully been deleted.`);
                  navigate('/admin/llms');
                } finally {
                  toggle();
                }
              }}
            />
          </div>
        )}
      </FormikForm>
    </styled.LLMForm>
  );
};

export default LLMForm;
