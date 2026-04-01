import { useFormikContext } from 'formik';
import { FaPaste } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Col,
  FieldSize,
  FormikText,
  FormikTextArea,
  type IReportModel,
  Row,
} from 'tno-core';

const system_prompt: string = `You are a media monitoring analyst producing a daily/weekly media environment summary for senior government decision-makers. Your audience includes Deputy Ministers, Assistant Deputy Ministers, and Minister’s Office staff. They use your output for situational awareness and to anticipate issues that may require a communications response.

Your role is to report, not to editorialize. You are a sensor, not a commentator.

Data Structure:
- The report data is split into sections.  Each section has a heading starting with #### [Section Name].  Each section contains stories in a JSON array.  The \`content.text\` property contains the story information.  The \`content.headline\` property is the story headline.

Follow these rules:
- The output generated must be in simple HTML that works within Outlook email client.
- Place all the output in a parent \`div\` HTML element.

TONE AND REGISTER:
- Write in a neutral, analytical register. Do not adopt the framing of any party, outlet, or stakeholder.
- Do not use intensifiers (mounting, escalating, alarming, significant, major, key) unless directly quoting a named source.
- Do not characterize public mood or sentiment unless citing specific evidence (polling, rally attendance, editorial board positions).
- Do not write narrative leads. The opening paragraph should list the topic clusters covered, not tell a story.

SPECIFICITY:
- Quantify wherever possible: counts, percentages, dollar figures, dates, community names.
- Where a specific number is not available, use square brackets to flag the gap: [number], [community], [date]. Do not substitute an adjective for a missing data point.
- Attribute all positions to named actors or organizations. Do not write “critics say” without identifying at least one critic.

STRUCTURE:
- Group coverage into topic clusters.
- Within each cluster, lead with the new development this cycle. Background and context follow, not the reverse.
- Use active voice with named subjects: “The Premier confirmed” not “it was confirmed.”
- Note when coverage threads intersect (e.g., affordability and housing appearing in the same stories).

WHAT NOT TO DO:
- Do not write as though you are a journalist composing a news story. You are producing an analytical briefing.
- Do not tell the reader how important, concerning, or significant a story is. Present the facts and let the reader assess significance.
- Do not use passive constructions that hide the actor: “concerns were raised” should become “[Organization] raised concerns about [specific issue].”
- Do not pad with filler language. If a topic cluster can be summarized in two sentences, use two sentences.`;
const user_prompt: string = `Create a concise summary within each section.

  Follow these rules:
  - Do not start the response with a heading.
  - Do not state your intention, only summarize the data. For example, do not say “this summary covers”.
  - Do not use the section headers in the data, instead group based on related topics.
  - Each section will begin with the topic cluster name, then have bullets for each story summary.
  - Use \`h3\` HTML tag for topic cluster headings. Only use the topic cluster headings from the report data, do not create other headings.
  - Do not use the story headline in the output.
  - Use \`li\` HTML tag in a \`ul\` HTML tag for each summary statement.`;

export interface IReportSectionAIProps {
  index: number;
}

export const ReportSectionAI = ({ index }: IReportSectionAIProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  return (
    <Col gap="1rem" className="section">
      <FormikText
        name={`sections.${index}.settings.label`}
        label="Heading"
        tooltip="A heading label to display at the beginning of the section"
        maxLength={100}
      />
      <FormikTextArea
        name={`sections.${index}.description`}
        label="Description"
        tooltip="Describe the purpose of this section"
        placeholder="An AI summary of the report"
      />
      <Row>
        <FormikText
          name={`sections.${index}.settings.deploymentName`}
          label="Model Deployment Name"
          tooltip="The name of the model deployment"
          maxLength={100}
          placeholder="gpt-5.1-chat"
        />
        <FormikText
          name={`sections.${index}.settings.temperature`}
          label="Temperature"
          tooltip="Apply randomness to responses. Depending on the model it may support values 0 - 2. Lower is more deterministic.  Higher is creative."
          type="number"
          width={FieldSize.Tiny}
          placeholder="0"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            const value = e.target.value;
            setFieldValue(
              `sections.${index}.settings.temperature`,
              value.trim() === '' ? undefined : +value,
            );
            console.debug(value);
          }}
        />
        <FormikText
          name={`sections.${index}.settings.choiceQty`}
          label="Choices"
          tooltip="Number of responses you want provided to compare.  Some models do not have this feature."
          type="number"
          width={FieldSize.Tiny}
          placeholder="1"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            const value = e.target.value;
            setFieldValue(
              `sections.${index}.settings.choiceQty`,
              value.trim() === '' ? undefined : +value,
            );
            console.debug(value);
          }}
        />
      </Row>
      <Row>
        <Col flex="1">
          <FormikTextArea
            name={`sections.${index}.settings.systemPrompt`}
            label="System Prompt"
            tooltip="Sets the global instructions and behavior for the model."
            placeholder={system_prompt}
          />
        </Col>
        <Col alignContent="center" justifyContent="center">
          <Button
            variant={ButtonVariant.link}
            title="Use default system prompt"
            onClick={() => setFieldValue(`sections.${index}.settings.systemPrompt`, system_prompt)}
          >
            <FaPaste />
          </Button>
        </Col>
      </Row>
      <Row>
        <Col flex="1">
          <FormikTextArea
            name={`sections.${index}.settings.userPrompt`}
            label="User Prompt"
            tooltip="Represents end-user input — questions, instructions, data, or prompts."
            placeholder={user_prompt}
          />
        </Col>
        <Col alignContent="center" justifyContent="center">
          <Button
            variant={ButtonVariant.link}
            title="Use default user prompt"
            onClick={() => setFieldValue(`sections.${index}.settings.userPrompt`, user_prompt)}
          >
            <FaPaste />
          </Button>
        </Col>
      </Row>
      <Checkbox
        name={`sections.${index}.settings.inTableOfContents`}
        label="Include in Table of Contents"
        checked={
          values.sections[index].settings.inTableOfContents === undefined
            ? true
            : values.sections[index].settings.inTableOfContents
        }
        onChange={(e) => {
          setFieldValue(`sections.${index}.settings.inTableOfContents`, e.target.checked);
        }}
      />
    </Col>
  );
};
