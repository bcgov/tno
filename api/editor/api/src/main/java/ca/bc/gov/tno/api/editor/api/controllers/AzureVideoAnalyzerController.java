package ca.bc.gov.tno.api.editor.api.controllers;

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

import ca.bc.gov.tno.api.editor.api.AzureVideoAnalyzerConfig;

@RestController
@RequestMapping("/azure/video/analyzer")
public class AzureVideoAnalyzerController {

	@Autowired
	private AzureVideoAnalyzerConfig config;

	@GetMapping(path = "/access-token")
	public String getAccessToken() throws InterruptedException, ExecutionException {
		HttpClient client = HttpClient.newHttpClient();
		HttpRequest request = HttpRequest.newBuilder()
				.uri(URI.create(String.format("https://api.videoindexer.ai/auth/%s/accounts/%s/accessToken?allowEdit=true",
						config.location, config.accountId)))
				.header("Ocp-Apim-Subscription-Key", config.subscriptionKey).build();
		Future<HttpResponse<String>> response = client.sendAsync(request, BodyHandlers.ofString());

		while (!response.isDone()) {
			// Wait
		}
		String token = response.get().body();
		return token;
	}

	@PostMapping(path = "/upload", consumes = { MediaType.MULTIPART_FORM_DATA_VALUE })
	public String uploadFile(@RequestPart(value = "file", required = true) MultipartFile files) throws IOException {

		return "test";
	}
}
