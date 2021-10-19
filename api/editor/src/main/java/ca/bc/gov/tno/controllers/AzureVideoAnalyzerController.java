package ca.bc.gov.tno.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.MediaType;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.net.http.HttpResponse.BodyHandlers;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

import ca.bc.gov.tno.azure.AzureVideoAnalyzerConfig;
import ca.bc.gov.tno.core.AsyncHelper;

/**
 * Endpoints to communicate with Microsoft Azure Video Analyzer API.
 */
@RestController
@RequestMapping("/azure/video/analyzer")
public class AzureVideoAnalyzerController {

	@Autowired
	private AzureVideoAnalyzerConfig config;

	private HttpClient client = HttpClient.newHttpClient();

	/**
	 * Request an access token to make requests to Azure Video Analyzer API.
	 *
	 * @return A bearer token.
	 * @throws InterruptedException
	 * @throws ExecutionException
	 */
	@GetMapping(path = "/access-token", produces = MediaType.TEXT_PLAIN_VALUE)
	public String getAccessToken() throws InterruptedException, ExecutionException {
		HttpRequest request = HttpRequest.newBuilder()
				.uri(URI.create(String.format("https://api.videoindexer.ai/auth/%s/accounts/%s/accessToken?allowEdit=true",
						config.location, config.accountId)))
				.header("Ocp-Apim-Subscription-Key", config.subscriptionKey).build();
		Future<HttpResponse<String>> response = client.sendAsync(request, BodyHandlers.ofString());

		String token = AsyncHelper.wait(response).get().body().replace("\"", "");

		return token;
	}

	/**
	 * Requests a list of videos.
	 *
	 * @return An object containing a page of videos.
	 * @throws IOException
	 * @throws InterruptedException
	 * @throws ExecutionException
	 */
	@GetMapping(path = "/videos", produces = MediaType.APPLICATION_JSON_VALUE)
	public String listVideos() throws IOException, InterruptedException, ExecutionException {
		String token = getAccessToken();
		HttpRequest request = HttpRequest.newBuilder()
				.uri(URI.create(
						String.format("https://api.videoindexer.ai/%s/accounts/%s/videos", config.location, config.accountId)))
				.header("Ocp-Apim-Subscription-Key", config.subscriptionKey)
				.header("Authorization", String.format("Bearer %s", token)).build();
		Future<HttpResponse<String>> response = client.sendAsync(request, BodyHandlers.ofString());
		String list = AsyncHelper.wait(response).get().body();
		return list;
	}

	/**
	 * Upload a video.
	 *
	 * @param files
	 * @return An object containing information about the new video.
	 * @throws IOException
	 * @throws InterruptedException
	 * @throws ExecutionException
	 */
	@PostMapping(path = "/upload", consumes = { MediaType.MULTIPART_FORM_DATA_VALUE })
	public String uploadFile(@RequestPart(value = "file", required = true) MultipartFile files)
			throws IOException, InterruptedException, ExecutionException {
		return "test";
	}
}
