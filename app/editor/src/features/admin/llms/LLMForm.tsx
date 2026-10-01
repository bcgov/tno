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
  FormikSelect,
  FormikText,
  FormikTextArea,
  FormikWysiwyg,
  IconButton,
  type ILLMModel,
  LabelPosition,
  Modal,
  OptionItem,
  Row,
  Show,
  useModal,
} from 'tno-core';
import { number, object, string } from 'yup';

import { defaultLLM } from './constants';
import * as styled from './styled';

const LLMSchema = object({
  name: string().required('Name is required'),
  contextWindow: number().optional().integer().positive('Must be greater than 0'),
  maxOutputTokens: number()
    .optional()
    .integer()
    .positive('Must be greater than 0')
    .test('less-than-context', 'Must be less than the context window', function (value) {
      const contextWindow = this.parent.contextWindow;
      return value === undefined || contextWindow === undefined || value < contextWindow;
    }),
  requestsPerMinute: number().optional().integer().min(0),
  tokensPerMinute: number().optional().integer().min(0),
  deploymentName: string().required('Deployment Name is required'),
  systemPrompt: string().required('Default System Prompt is required'),
});

const tokenEstimationOptions = [
  new OptionItem('Heuristic (characters ÷ 4)', 'Heuristic'),
  new OptionItem('o200k_base (GPT-4o, GPT-4.1, GPT-5, o-series)', 'o200k_base'),
  new OptionItem('cl100k_base (GPT-4, GPT-3.5)', 'cl100k_base'),
];

const toOptionalNumber = (value: string) => (value.trim() === '' ? undefined : Number(value));

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
              <p className="limits-hint">
                Report AI sections and Content-Analysis budget every request from these limits. A
                direct-model LLM needs a context window and a maximum output.
              </p>
              <Row gap="1rem">
                <FormikText
                  width={FieldSize.Small}
                  name="contextWindow"
                  label="Context Window (tokens)"
                  type="number"
                  min={1}
                  tooltip="The tokens the model reads and writes in one request"
                  onChange={(e) => setFieldValue('contextWindow', toOptionalNumber(e.target.value))}
                />
                <FormikText
                  width={FieldSize.Small}
                  name="maxOutputTokens"
                  label="Max Output (tokens)"
                  type="number"
                  min={1}
                  tooltip="The most tokens reserved for a response"
                  onChange={(e) =>
                    setFieldValue('maxOutputTokens', toOptionalNumber(e.target.value))
                  }
                />
                <FormikSelect
                  width={FieldSize.Big}
                  name="tokenEstimation"
                  label="Token Estimation"
                  tooltip="How tokens are counted for this model"
                  options={tokenEstimationOptions}
                  value={tokenEstimationOptions.find((o) => o.value === values.tokenEstimation)}
                  onChange={(o) =>
                    setFieldValue('tokenEstimation', (o as OptionItem)?.value ?? undefined)
                  }
                />
              </Row>
              <Row gap="1rem">
                <FormikText
                  width={FieldSize.Small}
                  name="requestsPerMinute"
                  label="Requests / Minute"
                  type="number"
                  min={0}
                  tooltip="Leave empty for no limit"
                  onChange={(e) =>
                    setFieldValue('requestsPerMinute', toOptionalNumber(e.target.value))
                  }
                />
                <FormikText
                  width={FieldSize.Small}
                  name="tokensPerMinute"
                  label="Tokens / Minute"
                  type="number"
                  min={0}
                  tooltip="Leave empty for no limit"
                  onChange={(e) =>
                    setFieldValue('tokensPerMinute', toOptionalNumber(e.target.value))
                  }
                />
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
