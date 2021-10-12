package ca.bc.gov.tno.api.editor.api.controllers;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.core.io.ByteArrayResource;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;

import com.azure.core.annotation.BodyParam;
import com.azure.storage.blob.BlobContainerClient;
import com.azure.storage.blob.BlobServiceClient;
import com.azure.storage.blob.specialized.BlockBlobClient;

import ca.bc.gov.tno.api.editor.azure.AzureBlobAdapter;

/**
 * Endpoints to interact with Microsoft Azure Storage API.
 */
@RestController
@RequestMapping("/azure/storage")
public class AzureStorageController {
	@Autowired
	AzureBlobAdapter azureBlobAdapter;

	@Autowired
	BlobServiceClient client;

	/**
	 * Upload a file to blob storage.
	 * 
	 * @param files
	 * @return
	 * @throws IOException
	 */
	@PostMapping(path = "/upload", consumes = { MediaType.MULTIPART_FORM_DATA_VALUE })
	public Map<String, String> uploadFile(@RequestPart(value = "file", required = true) MultipartFile files)
			throws IOException {
		String name = azureBlobAdapter.upload(files, "prefix");
		Map<String, String> result = new HashMap<>();
		result.put("key", name);
		return result;
	}

	/**
	 * Download the specified file from blob storage.
	 * 
	 * @param file
	 * @return
	 * @throws IOException
	 */
	@GetMapping(path = "/download")
	public ResponseEntity<ByteArrayResource> uploadFile(@RequestParam(value = "file") String file) throws IOException {
		byte[] data = azureBlobAdapter.getFile(file);
		ByteArrayResource resource = new ByteArrayResource(data);

		return ResponseEntity.ok().contentLength(data.length).header("Content-type", "application/octet-stream")
				.header("Content-disposition", "attachment; filename=\"" + file + "\"").body(resource);
	}

	/**
	 * Create a new container in blob storage.
	 * 
	 * @param name
	 * @return
	 */
	@PostMapping("/containers/{name}")
	public String createContainer(@PathVariable String name) {
		BlobContainerClient blobContainerClient = client.getBlobContainerClient(name);
		blobContainerClient.create();
		return name;
	}

	/**
	 * Upload text data to blob storage.
	 * 
	 * @param name
	 * @param data
	 * @return
	 * @throws IOException
	 */
	@PostMapping("/containers/{name}/upload")
	public String upload(@PathVariable String name, @BodyParam("text/plain") String data) throws IOException {
		BlockBlobClient blobClient = client.getBlobContainerClient(name).getBlobClient("test.txt").getBlockBlobClient();

		InputStream dataStream = new ByteArrayInputStream(data.getBytes(StandardCharsets.UTF_8));
		blobClient.upload(dataStream, data.length());
		dataStream.close();

		return name;
	}

	/**
	 * Download text data from blob storage.
	 * 
	 * @param name
	 * @return
	 * @throws IOException
	 */
	@GetMapping("/containers/{name}/download")
	public String download(@PathVariable String name) throws IOException {
		BlockBlobClient blobClient = client.getBlobContainerClient(name).getBlobClient("test.txt").getBlockBlobClient();

		int dataSize = (int) blobClient.getProperties().getBlobSize();
		ByteArrayOutputStream outputStream = new ByteArrayOutputStream(dataSize);

		try {
			blobClient.downloadStream(outputStream);
			return outputStream.toString();

		} finally {
			outputStream.close();
		}
	}
}
