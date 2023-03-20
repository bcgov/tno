import { useFormikContext } from 'formik';
import React from 'react';
import {
  Col,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IIngestModel,
  Row,
  Section,
  useFormikHelpers,
} from 'tno-core';

import { Languages, LoggingLevels, TimeZones } from './constants';
import * as styled from './styled';

export const VideoRPi: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);
  const logLevel = LoggingLevels.find((t) => t.value === values.configuration.logLevel);

  return (
    <styled.IngestType>
      <FormikText
        label="Device Hostname"
        name="configuration.hostname"
        value={values.configuration.hostname}
        tooltip="Only devices specifically with this hostname will run this ingest"
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        options={TimeZones}
        value={timeZone}
        required
      />
      <p>
        FFmpeg is used to capture audio and video content. For more advanced configuration review{' '}
        <a href="https://www.ffmpeg.org/ffmpeg.html" target="_blank" rel="noreferrer">
          ffmpeg.org
        </a>
        .
      </p>
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="Language"
            name="configuration.language"
            options={Languages}
            value={language}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikSelect
            label="Logging Level"
            name="configuration.logLevel"
            options={LoggingLevels}
            value={logLevel}
          />
        </Col>
      </Row>
      <Section>
        <h3>Video Input</h3>
        <Row>
          <FormikText
            label="Input Device"
            name="configuration.videoInput"
            value={values.configuration.videoInput}
            tooltip="The input device for video"
            placeholder="/dev/video0"
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Thread Queue Size"
            name="configuration.videoThreadQueueSize"
            value={values.configuration.videoThreadQueueSize}
            type="number"
            placeholder="1024"
            tooltip="Sets the maximum number of queued packets when reading from the file or device. With low latency / high rate live streams, packets may be discarded if they are not read in a timely manner; setting this value can force ffmpeg to use a separate input thread and read packets as soon as they arrive. By default ffmpeg only does this if multiple inputs are specified."
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Format"
            name="configuration.videoInputFormat"
            value={values.configuration.videoInputFormat}
            placeholder="v4l2"
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Framerate"
            name="configuration.videoFramerate"
            value={values.configuration.videoFramerate}
            type="number"
            placeholder="30"
            size={5}
            onClick={applyPlaceholder}
          />
        </Row>
        <FormikText
          label="Other Arguments"
          name="configuration.videoArgs"
          value={values.configuration.videoArgs}
        />
      </Section>
      <Section>
        <h3>Audio Input</h3>
        <Row>
          <FormikText
            label="Input Device"
            name="configuration.audioInput"
            tooltip="The input device for audio"
            placeholder="hw:CARD=HDMI,DEV=0"
            value={values.configuration.audioInput}
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Thread Queue Size"
            name="configuration.audioThreadQueueSize"
            value={values.configuration.audioThreadQueueSize}
            type="number"
            placeholder="1024"
            tooltip="Sets the maximum number of queued packets when reading from the file or device. With low latency / high rate live streams, packets may be discarded if they are not read in a timely manner; setting this value can force ffmpeg to use a separate input thread and read packets as soon as they arrive. By default ffmpeg only does this if multiple inputs are specified."
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Format"
            name="configuration.audioInputFormat"
            value={values.configuration.audioInputFormat}
            placeholder="alsa"
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Channels"
            name="configuration.audioChannels"
            value={values.configuration.audioChannels}
            type="number"
            placeholder="2"
            size={5}
            onClick={applyPlaceholder}
          />
          <FormikText
            label="Channel Layout"
            name="configuration.audioChannelLayout"
            value={values.configuration.audioChannelLayout}
            placeholder="stereo"
            onClick={applyPlaceholder}
          />
        </Row>
        <FormikText
          label="Other Arguments"
          name="configuration.audioArgs"
          value={values.configuration.audioArgs}
        />
      </Section>
      <Section>
        <h3>Output</h3>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Thread Queue Size"
              name="configuration.outputThreadQueueSize"
              value={values.configuration.outputThreadQueueSize}
              type="number"
              tooltip="For output, this option specified the maximum number of packets that may be queued to each muxing thread."
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Volume"
              name="configuration.volume"
              value={values.configuration.volume}
              tooltip="Volume in percent or dB (1 = 100%)"
              placeholder="1"
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Format"
              name="configuration.outputFormat"
              value={values.configuration.outputFormat}
              tooltip="Format of the output file"
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Buffer Size"
              name="configuration.bufferSize"
              value={values.configuration.bufferSize}
              tooltip=""
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Buffer Min Rate"
              name="configuration.minRate"
              value={values.configuration.minRate}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Buffer Max Rate"
              name="configuration.maxRate"
              value={values.configuration.maxRate}
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Audio Encoder"
              name="configuration.audioEncoder"
              value={values.configuration.audioEncoder}
              tooltip="Select an encoder codec or a special value 'copy' to indicate that the stream is not to be re-encoded"
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Audio Buffer Size"
              name="configuration.audioBufferSize"
              value={values.configuration.audioBufferSize}
              onClick={applyPlaceholder}
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Video Encoder"
              name="configuration.videoEncoder"
              value={values.configuration.videoEncoder}
              tooltip="Select an encoder codec or a special value 'copy' to indicate that the stream is not to be re-encoded"
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Video Buffer Size"
              name="configuration.videoBufferSize"
              value={values.configuration.videoBufferSize}
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Scaling"
              name="configuration.scale"
              value={values.configuration.scale}
              tooltip="The format is ‘wxh’ (default - same as source)"
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Pixel Format"
              name="configuration.pixelFormat"
              value={values.configuration.pixelFormat}
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Frame Rate"
              name="configuration.frameRate"
              value={values.configuration.frameRate}
              type="number"
              tooltip="To force the frame rate of the output file"
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="Keyframe"
              name="configuration.keyframe"
              value={values.configuration.keyframe}
              type="number"
              tooltip="Keyframe interval. A keyframe is inserted at least every -g frames, sometimes sooner."
              onClick={applyPlaceholder}
            />
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 0">
            <FormikText
              label="Preset"
              name="configuration.preset"
              value={values.configuration.preset}
              tooltip="Use the preset parameter to control the speed of the compression process."
              onClick={applyPlaceholder}
            />
          </Col>
          <Col flex="1 1 0">
            <FormikText
              label="CRF"
              name="configuration.crf"
              value={values.configuration.crf}
              type="number"
              tooltip="Use the CRF (Constant Rate Factor) parameter to control the output quality. The lower crf, the higher the quality (range: 0-51). The default value is 23, and visually lossless compression corresponds to -crf 18."
              onClick={applyPlaceholder}
            />
          </Col>
        </Row>
        <FormikTextArea
          label="Other Arguments"
          name="configuration.otherArgs"
          value={values.configuration.otherArgs}
          tooltip="Any other arguments to pass to the command"
          onClick={applyPlaceholder}
        />
        <p>Use "{'{schedule.Name}.mkv'}" to name the file with the schedule name.</p>
        <FormikText
          label="File Name"
          name="configuration.fileName"
          value={values.configuration.fileName}
          placeholder="{schedule.Name}.mkv"
          onClick={applyPlaceholder}
        />
      </Section>
    </styled.IngestType>
  );
};
