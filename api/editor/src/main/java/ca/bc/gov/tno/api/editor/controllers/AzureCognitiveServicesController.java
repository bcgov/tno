package ca.bc.gov.tno.api.editor.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.io.InputStream;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Semaphore;

import com.microsoft.cognitiveservices.speech.CancellationReason;
import com.microsoft.cognitiveservices.speech.ResultReason;
import com.microsoft.cognitiveservices.speech.SpeechConfig;
import com.microsoft.cognitiveservices.speech.SpeechRecognizer;
import com.microsoft.cognitiveservices.speech.audio.AudioConfig;
import com.microsoft.cognitiveservices.speech.audio.AudioInputStream;
import com.microsoft.cognitiveservices.speech.audio.AudioStreamFormat;
import com.microsoft.cognitiveservices.speech.audio.PushAudioInputStream;

/**
 * Endpoints to communicate with Microsoft Azure Cognitive services API.
 */
@RestController
@RequestMapping("/azure/cognitive/services")
public class AzureCognitiveServicesController {
	@Autowired
	SpeechConfig speechConfig;

	/**
	 * Upload an audio file and transcribe it.
	 * 
	 * @param files
	 * @return Transcription of audio file.
	 * @throws InterruptedException
	 * @throws ExecutionException
	 * @throws IOException
	 */
	@PostMapping(path = "/transcribe", consumes = { MediaType.MULTIPART_FORM_DATA_VALUE })
	public String uploadFile(@RequestPart(value = "file", required = true) MultipartFile files)
			throws InterruptedException, ExecutionException, IOException {

		AudioStreamFormat format = AudioStreamFormat.getWaveFormatPCM(40000L, (short) 16, (short) 2);
		PushAudioInputStream audioStream = AudioInputStream.createPushStream(format);
		InputStream fileStream = files.getInputStream();
		audioStream.write(files.getBytes());

		AudioConfig audioConfig = AudioConfig.fromStreamInput(audioStream);
		SpeechRecognizer recognizer = new SpeechRecognizer(speechConfig, audioConfig);

		try {
			Semaphore stop = new Semaphore(0);
			recognizer.startContinuousRecognitionAsync().get();

			StringBuilder text = new StringBuilder();

			recognizer.recognized.addEventListener((o, e) -> {
				if (e.getResult().getReason() == ResultReason.RecognizedSpeech) {
					System.out.println("RECOGNIZED: Text=" + e.getResult().getText());
					text.append(e.getResult().getText());
				} else if (e.getResult().getReason() == ResultReason.NoMatch) {
					System.out.println("RECOGNIZED: Text=" + e.getResult().getText());
					text.append("NoMatch");
				}
			});
			recognizer.recognizing.addEventListener((o, e) -> {
				System.out.println("RECOGNIZING: Text=" + e.getResult().getText());
			});
			recognizer.speechStartDetected.addEventListener((o, e) -> {
				System.out.println("SPEECH START DETECTED");
			});
			recognizer.speechEndDetected.addEventListener((o, e) -> {
				System.out.println("SPEECH END DETECTED");
				stop.release();
			});
			recognizer.canceled.addEventListener((o, e) -> {
				if (e.getReason() == CancellationReason.Error) {
					text.append("Failed:" + e.getReason() + " [" + e.getErrorCode() + "] - " + e.getErrorDetails());
				} else if (e.getReason() == CancellationReason.CancelledByUser) {
					System.out.println("CANCELED BY USER");
				} else if (e.getReason() == CancellationReason.EndOfStream) {
					System.out.println("END OF STREAM");
				}
				stop.release();
			});
			recognizer.sessionStarted.addEventListener((o, e) -> {
				System.out.println("SESSION STARTED");
			});
			recognizer.sessionStopped.addEventListener((o, e) -> {
				System.out.println("SESSION STOPPED");
				stop.release();
			});

			audioStream.close();
			stop.acquire();
			recognizer.stopContinuousRecognitionAsync().get();

			audioStream.close();
			fileStream.close();

			return text.toString();
		} finally {
			audioConfig.close();
			recognizer.close();
		}
	}
}